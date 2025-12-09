using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using RevivalMod.Features;

namespace RevivalMod.Patches
{
    /// <summary>
    /// Prevents players from shooting while in critical (downed) state.
    /// Patches Player.FirearmController.InitiateShot to block the shot initiation.
    /// </summary>
    internal class DisableShootingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.InitiateShot));
        }

        [PatchPrefix]
        private static bool Prefix(Player ____player)
        {
            try
            {
                if (____player == null || ____player.IsAI)
                    return true; // Allow AI to shoot normally
                
                // Check if player is in critical state
                if (RevivalFeatures.IsPlayerInCriticalState(____player.ProfileId))
                {
                    // Block the shot - player is downed
                    return false;
                }
            }
            catch
            {
                // If we can't determine state, allow the shot
            }
            
            return true; // Allow normal shooting
        }
    }
}

