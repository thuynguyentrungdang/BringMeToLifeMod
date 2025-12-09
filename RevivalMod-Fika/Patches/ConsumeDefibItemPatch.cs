using System;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Players;
using HarmonyLib;
using SPT.Reflection.Patching;
using RevivalMod.Features;

namespace RevivalMod.Patches;

[HarmonyPatch(typeof(RevivalFeatures))]
[HarmonyPatch(nameof(RevivalFeatures.ConsumeDefibItem))]
public class ConsumeDefibItemPatch
{
    [PatchPrefix]
    private static void PatchPrefix(Player ___player, Item ___defibItem)
    {
        try
        {
            if (___player is not FikaPlayer fikaPlayer)
                return;

            InventoryController inventoryController = fikaPlayer.InventoryController;
            GStruct153 discardResult = InteractionsHandlerClass.Discard(___defibItem, inventoryController, true);

            if (discardResult.Failed)
            {
                Plugin.LogSource.LogError($"Couldn't remove item: {discardResult.Error}");
                return;
            }
                
            inventoryController.TryRunNetworkTransaction(discardResult, result =>
            {
                if (result.Failed)
                    Plugin.LogSource.LogError($"inventoryController.TryRunNetworkTransaction failed: {result.Error}");
            });
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"Error consuming defib item: {ex.Message}");
        }
    }
}