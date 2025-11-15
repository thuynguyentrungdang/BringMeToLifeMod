using EFT;
using Comfort.Common;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;
using RevivalMod.Features;
using RevivalMod.Components;

namespace RevivalMod.Patches
{
    internal class GameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            try
            {
                Plugin.LogSource.LogInfo("Game started");

                if (Application.isBatchMode)
                {
                    Plugin.LogSource.LogInfo("Running on headless, skipping OnGameStarted patch...");
                    return;
                }

                // Make sure GameWorld is instantiated
                if (!Singleton<GameWorld>.Instantiated)
                {
                    Plugin.LogSource.LogError("GameWorld not instantiated yet");
                    return;
                }

                // Initialize player client directly
                Player playerClient = Singleton<GameWorld>.Instance.MainPlayer;

                if (playerClient is null)
                {
                    Plugin.LogSource.LogError("MainPlayer is null");
                    return;
                }

                RevivalFeatures._playerList[playerClient.ProfileId] = new RMPlayer();

                // Enable interactable
                Plugin.LogSource.LogDebug("Enabling body interactable");

                foreach (GameObject interact in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (!interact.name.Contains("Body Interactable")) 
                        continue;
                    
                    Plugin.LogSource.LogDebug($"Found interactable: {interact.name}");
                    
                    interact.layer = LayerMask.NameToLayer("Interactive");
                    interact.GetComponent<BoxCollider>().enabled = true;
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error in GameStartedPatch: {ex.Message}");
            }
        }
    }
}