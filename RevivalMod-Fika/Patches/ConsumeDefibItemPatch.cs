using System;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Players;
using HarmonyLib;
using SPT.Reflection.Patching;
using RevivalMod.Features;
using RevivalMod.Helpers;

namespace RevivalMod.Patches;

[HarmonyPatch(typeof(RevivalFeatures))]
[HarmonyPatch(nameof(RevivalFeatures.ConsumeDefibItem))]
public class ConsumeDefibItemPatch
{
    [PatchPrefix]
    private static void PatchPrefix(Player ___player, Item ___defibItem)
    {
        if (RevivalModSettings.KEEP_DEFIB_ITEM == true) {
            return;
        }
        try
        {
            if (___player is not FikaPlayer fikaPlayer)
                return;

            if (___defibItem == null)
                return;

            FikaPlayer fikaPlayer = (FikaPlayer)___player;
            InventoryController inventoryController = fikaPlayer.InventoryController;
            
            // Discard the item
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