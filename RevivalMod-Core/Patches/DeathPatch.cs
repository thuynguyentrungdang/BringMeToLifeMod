using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using RevivalMod.Features;
using RevivalMod.Helpers;
using UnityEngine;
using EFT.Communications;

namespace RevivalMod.Patches
{
    internal class DeathPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.Kill));
        }

        [PatchPrefix]
        private static bool Prefix(ActiveHealthController __instance, EDamageType damageType)
        {
            try
            {
                // Get the Player field
                FieldInfo playerField = AccessTools.Field(typeof(ActiveHealthController), "Player");
                
                if (playerField?.GetValue(__instance) is not Player player || 
                    player.IsAI) 
                    return true;

                string playerId = player.ProfileId;

                // Check for explicit kill override
                if (RevivalFeatures._playerList[playerId].KillOverride)
                    return true;
                
                // Check if player is already in critical state & God Mode is off
                if (RevivalFeatures._playerList[playerId].IsCritical && 
                    !RevivalModSettings.GOD_MODE)
                    return true;

                // Check if player is invulnerable from recent revival
                if (RevivalFeatures.IsPlayerInvulnerable(playerId))
                {
                    Plugin.LogSource.LogDebug($"Player {playerId} is invulnerable, blocking death completely");
                    return false; // Block the kill completely
                }

                // If player dies again too fast, kill them.
                if (RevivalFeatures.IsRevivalOnCooldown(playerId))
                    return true;

                Plugin.LogSource.LogInfo($"DEATH PREVENTION: Player {player.ProfileId} about to die from {damageType}");

                // Check for headshot instant death
                if (RevivalModSettings.HARDCORE_HEADSHOT_DEFAULT_DEAD &&
                    __instance.GetBodyPartHealth(EBodyPart.Head, true).Current < 1 &&
                    damageType == EDamageType.Bullet)
                {
                    // Handle random chance of critical state.
                    float randomNumber = UnityEngine.Random.Range(0f, 100f);

                    if (randomNumber < RevivalModSettings.HARDCORE_CHANCE_OF_CRITICAL_STATE)
                    {
                        Plugin.LogSource.LogInfo($"DEATH PREVENTED: Player was lucky. Random Number was: {randomNumber}");

                        NotificationManagerClass.DisplayMessageNotification(
                            "Headshot - critical",
                            ENotificationDurationType.Default,
                            ENotificationIconType.Alert,
                            Color.green);
                    }
                    else
                    {
                        Plugin.LogSource.LogInfo($"DEATH NOT PREVENTED: Player headshotted");

                        NotificationManagerClass.DisplayMessageNotification(
                            "Headshot - killed instantly",
                            ENotificationDurationType.Default,
                            ENotificationIconType.Alert,
                            Color.red);

                        return true; // Allow death to happen normally
                    }
                }

                // At this point, we want the player to enter critical state
                RevivalFeatures.SetPlayerCriticalState(player, true, damageType);

                // Block the kill completely
                return false;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error in Death prevention patch: {ex.Message}");
            }

            return true;
        }
    }
}