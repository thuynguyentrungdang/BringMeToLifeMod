using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.UI;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using HarmonyLib;
using RevivalMod.Components;
using RevivalMod.Features;
using RevivalMod.FikaModule.Packets;
using RevivalMod.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;

namespace RevivalMod.FikaModule.Common
{
    internal class FikaMethods
    {
        // Track players currently in ghost mode - accessible for patches
        public static HashSet<string> PlayersInGhostMode { get; } = new HashSet<string>();
        
        // Harmony instance for patching SAIN
        private static Harmony _harmonyInstance;
        
        /// <summary>
        /// Initialize Harmony patches for SAIN ghost mode integration
        /// </summary>
        public static void InitSAINPatches()
        {
            try
            {
                // Try to find SAIN's Enemy.IsEnemyValid method and patch it
                var sainEnemyType = Type.GetType("SAIN.SAINComponent.Classes.EnemyClasses.Enemy, SAIN");
                if (sainEnemyType == null)
                {
                    Plugin.LogSource.LogInfo("[GhostMode] SAIN not found, skipping SAIN patches");
                    return;
                }

                var isEnemyValidMethod = sainEnemyType.GetMethod("IsEnemyValid", 
                    BindingFlags.Public | BindingFlags.Static);
                
                if (isEnemyValidMethod == null)
                {
                    Plugin.LogSource.LogWarning("[GhostMode] Could not find SAIN Enemy.IsEnemyValid method");
                    return;
                }

                _harmonyInstance = new Harmony("com.revivalmod.sainghostmode");
                
                var postfixMethod = typeof(FikaMethods).GetMethod(nameof(IsEnemyValidPostfix), 
                    BindingFlags.NonPublic | BindingFlags.Static);
                
                _harmonyInstance.Patch(isEnemyValidMethod, postfix: new HarmonyMethod(postfixMethod));
                
                Plugin.LogSource.LogInfo("[GhostMode] Successfully patched SAIN Enemy.IsEnemyValid for ghost mode!");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[GhostMode] Failed to patch SAIN: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Harmony postfix for SAIN's Enemy.IsEnemyValid - returns false for ghost mode players
        /// </summary>
        private static void IsEnemyValidPostfix(string botProfileId, object enemyPlayerComp, ref bool __result)
        {
            // If already invalid, don't need to check
            if (!__result) return;
            
            try
            {
                // Get the Player from the PlayerComponent
                var playerProp = enemyPlayerComp?.GetType().GetProperty("Player");
                if (playerProp == null) return;
                
                var player = playerProp.GetValue(enemyPlayerComp) as Player;
                if (player == null) return;
                
                // Check if this player is in ghost mode
                if (PlayersInGhostMode.Contains(player.ProfileId))
                {
                    __result = false;
                    // Only log occasionally to avoid spam
                    // Plugin.LogSource.LogDebug($"[GhostMode] SAIN patch: Marking player {player.ProfileId} as invalid (ghost mode)");
                }
            }
            catch
            {
                // Ignore reflection errors
            }
        }

        public static void SendPlayerPositionPacket(string playerId, DateTime timeOfDeath, Vector3 position)
        {
            PlayerPositionPacket packet = new()
            {
                playerId = playerId,
                timeOfDeath = timeOfDeath,
                position = position
            };

            if (FikaBackendUtils.IsServer)
            {
                try
                {
                    Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError(ex);
                }
            }
            else if (FikaBackendUtils.IsClient)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableSequenced);
            }

        }
                
        public static void SendRemovePlayerFromCriticalPlayersListPacket(string playerId)
        {
            RemovePlayerFromCriticalPlayersListPacket packet = new()
            {
                playerId = playerId
            };

            if (FikaBackendUtils.IsServer)
            {
                try
                {
                    Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError(ex);
                }
            }
            else if (FikaBackendUtils.IsClient)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableSequenced);
            }
        }

        public static void SendReviveMePacket(string reviveeId, string reviverId)
        {
            ReviveMePacket packet = new()
            {
                reviverId = reviverId,
                reviveeId = reviveeId
            };

            if (FikaBackendUtils.IsServer)
            {               
                try
                {
                    Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError(ex);
                }
            }
            else if (FikaBackendUtils.IsClient)
            {              
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableSequenced);
            }
        }

        public static void SendReviveSucceedPacket(string reviverId, NetPeer peer)
        {
            RevivedPacket packet = new()
            {
                reviverId = reviverId
            };

            if (FikaBackendUtils.IsServer)
            {
                try
                {
                    Singleton<FikaServer>.Instance.SendDataToPeer(ref packet, DeliveryMethod.ReliableOrdered, peer);
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError(ex);
                }
            }
            else if (FikaBackendUtils.IsClient)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableSequenced);
            }

        }

        public static void SendReviveStartedPacket(string reviveeId, string reviverId)
        {
            ReviveStartedPacket packet = new()
            {
                reviverId = reviverId,
                reviveeId = reviveeId
            };

            if (FikaBackendUtils.IsServer)
            {
                try
                {
                    Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError(ex);
                }
            }
            else if (FikaBackendUtils.IsClient)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableSequenced);
            }
        }
        
        public static void SendReviveCanceledPacket(string reviveeId, string reviverId)
        {
            ReviveCanceledPacket packet = new()
            {
                reviverId = reviverId,
                reviveeId = reviveeId
            };

            if (FikaBackendUtils.IsServer)
            {               
                try
                {
                    Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError(ex);
                }
            }
            else if (FikaBackendUtils.IsClient)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableSequenced);
            }
        }

        public static void SendPlayerGhostModePacket(string playerId, bool isAlive)
        {
            PlayerGhostModePacket packet = new()
            {
                playerId = playerId,
                isAlive = isAlive
            };

            if (FikaBackendUtils.IsServer)
            {
                try
                {
                    Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError(ex);
                }
            }
            else if (FikaBackendUtils.IsClient)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableSequenced);
            }
        }

        private static void OnPlayerPositionPacketReceived(PlayerPositionPacket packet, NetPeer peer)
        {
            // Server (player host or headless) always forwards to all clients
            if (FikaBackendUtils.IsServer)
            {
                SendPlayerPositionPacket(packet.playerId, packet.timeOfDeath, packet.position);
            }
            
            // Non-headless machines (player hosts and clients) process the packet
            if (!FikaBackendUtils.IsHeadless)
            {
                RMSession.AddToCriticalPlayers(packet.playerId, packet.position);
            }
        }
        
        private static void OnRemovePlayerFromCriticalPlayersListPacketReceived(RemovePlayerFromCriticalPlayersListPacket packet, NetPeer peer)
        {
            // Server (player host or headless) always forwards to all clients
            if (FikaBackendUtils.IsServer)
            {
                SendRemovePlayerFromCriticalPlayersListPacket(packet.playerId);
            }
            
            // Non-headless machines (player hosts and clients) process the packet
            if (!FikaBackendUtils.IsHeadless)
            {
                RMSession.RemovePlayerFromCriticalPlayers(packet.playerId);
            }
        }
        
        /// <summary>
        /// Handles the reception of a <see cref="ReviveMePacket"/> from a network peer.
        /// Depending on the server state and backend configuration, either forwards the revive request
        /// or attempts to perform a revival by a teammate. If the revival is successful, sends a notification
        /// packet to the reviver.
        /// </summary>
        /// <param name="packet">The <see cref="ReviveMePacket"/> containing revivee and reviver IDs.</param>
        /// <param name="peer">The <see cref="NetPeer"/> that sent the packet.</param>
        private static void OnReviveMePacketReceived(ReviveMePacket packet, NetPeer peer)
        {
            // Server (player host or headless) always forwards to all clients
            if (FikaBackendUtils.IsServer)
            {
                SendReviveMePacket(packet.reviveeId, packet.reviverId);
            }
            
            // Non-headless machines (player hosts and clients) process the packet
            if (!FikaBackendUtils.IsHeadless)
            {
                bool revived = RevivalFeatures.TryPerformRevivalByTeammate(packet.reviveeId);
                
                if (!revived) 
                    return;
                
                SendReviveSucceedPacket(packet.reviverId, peer);
                Singleton<GameUI>.Instance.BattleUiPanelExtraction.Close();
            }
        }

        private static void OnReviveSucceedPacketReceived(RevivedPacket packet, NetPeer peer)
        {
            // Server (player host or headless) always forwards to all clients
            if (FikaBackendUtils.IsServer)
            {
                SendReviveSucceedPacket(packet.reviverId, peer);
            }
            
            // Non-headless machines (player hosts and clients) process the packet
            if (!FikaBackendUtils.IsHeadless)
            {
                NotificationManagerClass.DisplayMessageNotification(
                        $"Successfully revived your teammate!",
                        ENotificationDurationType.Long,
                        ENotificationIconType.Friend,
                        Color.green);
            }
        }

        private static void OnReviveStartedPacketReceived(ReviveStartedPacket packet, NetPeer peer)
        {
            // Server (player host or headless) always forwards to all clients
            if (FikaBackendUtils.IsServer)
            {
                SendReviveStartedPacket(packet.reviveeId, packet.reviverId);
            }
            
            // Non-headless machines (player hosts and clients) process the packet
            if (!FikaBackendUtils.IsHeadless)
            {
                if (FikaBackendUtils.Profile.ProfileId != packet.reviveeId)
                    return;

                Plugin.LogSource.LogDebug("ReviveStarted packet received");
                
                RevivalFeatures.criticalStateMainTimer.StopTimer();
                Singleton<GameUI>.Instance.BattleUiPanelExtraction.Display();
                
                TextMeshProUGUI textTimerPanel = MonoBehaviourSingleton<GameUI>.Instance.BattleUiPanelExtraction.GetComponentInChildren<TextMeshProUGUI>();
                
                textTimerPanel.SetText("Being revived...");
            }
        }

        private static void OnReviveCanceledPacketReceived(ReviveCanceledPacket packet, NetPeer peer)
        {
            // Server (player host or headless) always forwards to all clients
            if (FikaBackendUtils.IsServer)
            {
                SendReviveCanceledPacket(packet.reviveeId, packet.reviverId);
            }
            
            // Non-headless machines (player hosts and clients) process the packet
            if (!FikaBackendUtils.IsHeadless)
            {
                if (FikaBackendUtils.Profile.ProfileId != packet.reviveeId)
                    return;
                    
                Plugin.LogSource.LogDebug("ReviveCanceled packet received");

                Singleton<GameUI>.Instance.BattleUiPanelExtraction.Close();

                RevivalFeatures.criticalStateMainTimer.StartCountdown(RevivalFeatures._playerList[packet.reviveeId].CriticalTimer,
                                                                    "Critical State Timer", TimerPosition.MiddleCenter);
            }
        }

        private static void OnPlayerGhostModePacketReceived(PlayerGhostModePacket packet, NetPeer peer)
        {
            Plugin.LogSource.LogInfo($"[GhostMode] Packet received: playerId={packet.playerId}, isAlive={packet.isAlive}, IsServer={FikaBackendUtils.IsServer}, IsHeadless={FikaBackendUtils.IsHeadless}");

            // Server (player host or headless) always forwards to all clients
            if (FikaBackendUtils.IsServer)
            {
                SendPlayerGhostModePacket(packet.playerId, packet.isAlive);
            }
            
            // All machines process ghost mode state (needed for AI targeting)
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Plugin.LogSource.LogError("[GhostMode] GameWorld is null!");
                return;
            }

            // Find the target player
            Player targetPlayer = gameWorld.GetEverExistedPlayerByID(packet.playerId);
            
            if (targetPlayer == null)
            {
                Plugin.LogSource.LogWarning($"[GhostMode] GetEverExistedPlayerByID returned null for {packet.playerId}");
                return;
            }

            Plugin.LogSource.LogInfo($"[GhostMode] Found player: ProfileId={targetPlayer.ProfileId}, IsAI={targetPlayer.IsAI}, Type={targetPlayer.GetType().Name}");

            // NOTE: We do NOT modify IsAlive - that breaks Fika's death/extraction sync on headless
            // Instead, we ONLY use the PlayersInGhostMode HashSet which SAIN checks via our patch
            
            // Track ghost mode state - this is used by the SAIN patch to mark enemies as invalid
            if (!packet.isAlive)
            {
                // Player is entering ghost mode
                PlayersInGhostMode.Add(packet.playerId);
                Plugin.LogSource.LogInfo($"[GhostMode] Player {packet.playerId} added to ghost mode list ({PlayersInGhostMode.Count} total in ghost mode)");
                
                // Also do immediate removal from bot enemy lists for faster effect
                RemovePlayerFromAllBotEnemies(targetPlayer, gameWorld);
            }
            else
            {
                // Player is exiting ghost mode (revived or died)
                bool wasInGhostMode = PlayersInGhostMode.Remove(packet.playerId);
                Plugin.LogSource.LogInfo($"[GhostMode] Player {packet.playerId} removed from ghost mode list (was in list: {wasInGhostMode}). Remaining in ghost mode: {PlayersInGhostMode.Count}");
                
                // Ensure any residual state is cleaned up
                // Note: For extraction to work properly after death, the player must NOT be in ghost mode
                if (wasInGhostMode)
                {
                    Plugin.LogSource.LogInfo($"[GhostMode] Player {packet.playerId} ghost mode fully cleared - extraction should work");
                }
            }
        }

        /// <summary>
        /// Removes a player from all bot enemy memory so they stop targeting them.
        /// This works with both vanilla AI and SAIN.
        /// </summary>
        private static void RemovePlayerFromAllBotEnemies(Player targetPlayer, GameWorld gameWorld)
        {
            if (targetPlayer == null || gameWorld == null)
                return;

            int botsCleared = 0;
            int sainBotsCleared = 0;
            
            try
            {
                // Get all registered players and find bots
                foreach (Player player in gameWorld.RegisteredPlayers)
                {
                    if (player == null || !player.IsAI)
                        continue;

                    // Get the bot's AIData which contains the BotOwner
                    if (player.AIData?.BotOwner == null)
                        continue;

                    BotOwner botOwner = player.AIData.BotOwner;

                    try
                    {
                        // 1. Clear from vanilla AI goal enemy if this is the current target
                        if (botOwner.Memory?.GoalEnemy?.Person?.ProfileId == targetPlayer.ProfileId)
                        {
                            botOwner.Memory.GoalEnemy = null;
                            botsCleared++;
                            Plugin.LogSource.LogInfo($"[GhostMode] Cleared vanilla GoalEnemy for bot {player.ProfileId}");
                        }

                        // 2. Try to access SAIN's BotComponent and call RemoveEnemy
                        // SAIN adds a BotComponent to each bot's GameObject
                        if (TryRemoveFromSAIN(botOwner, targetPlayer.ProfileId))
                        {
                            sainBotsCleared++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogSource.LogWarning($"[GhostMode] Error clearing bot {player.ProfileId}: {ex.Message}");
                    }
                }

                Plugin.LogSource.LogInfo($"[GhostMode] Cleared {botsCleared} vanilla bots, {sainBotsCleared} SAIN bots for player {targetPlayer.ProfileId}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GhostMode] Error in RemovePlayerFromAllBotEnemies: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to remove the target player from SAIN's enemy tracking via reflection.
        /// Returns true if SAIN was found and the enemy was removed.
        /// </summary>
        private static bool TryRemoveFromSAIN(BotOwner botOwner, string targetProfileId)
        {
            try
            {
                // Try to get SAIN's BotComponent from the bot's GameObject
                // SAIN adds this component to each bot
                var botGameObject = botOwner.gameObject;
                if (botGameObject == null)
                    return false;

                // Use reflection to find SAIN.Components.BotComponent
                var sainBotComponent = botGameObject.GetComponent("BotComponent");
                if (sainBotComponent == null)
                    return false;

                // Get the EnemyController property
                var enemyControllerProp = sainBotComponent.GetType().GetProperty("EnemyController");
                if (enemyControllerProp == null)
                    return false;

                var enemyController = enemyControllerProp.GetValue(sainBotComponent);
                if (enemyController == null)
                    return false;

                // Call RemoveEnemy(profileId) method
                var removeEnemyMethod = enemyController.GetType().GetMethod("RemoveEnemy", new[] { typeof(string) });
                if (removeEnemyMethod == null)
                    return false;

                removeEnemyMethod.Invoke(enemyController, new object[] { targetProfileId });
                Plugin.LogSource.LogInfo($"[GhostMode] Called SAIN RemoveEnemy for profile {targetProfileId}");
                return true;
            }
            catch (Exception ex)
            {
                // SAIN not installed or reflection failed - this is fine, just use vanilla AI clearing
                Plugin.LogSource.LogDebug($"[GhostMode] SAIN integration not available: {ex.Message}");
                return false;
            }
        }

        public static void OnFikaNetManagerCreated(FikaNetworkManagerCreatedEvent managerCreatedEvent)
        {
            Plugin.LogSource.LogInfo("[Fika Module] OnFikaNetManagerCreated - Registering packet handlers...");
            managerCreatedEvent.Manager.RegisterPacket<PlayerPositionPacket, NetPeer>(OnPlayerPositionPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<RemovePlayerFromCriticalPlayersListPacket, NetPeer>(OnRemovePlayerFromCriticalPlayersListPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<ReviveMePacket, NetPeer>(OnReviveMePacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<RevivedPacket, NetPeer>(OnReviveSucceedPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<ReviveStartedPacket, NetPeer>(OnReviveStartedPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<ReviveCanceledPacket, NetPeer>(OnReviveCanceledPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<PlayerGhostModePacket, NetPeer>(OnPlayerGhostModePacketReceived);
            Plugin.LogSource.LogInfo("[Fika Module] All packet handlers registered (including GhostMode)!");
        }
        
        public static void InitOnPluginEnabled()
        {
            Plugin.LogSource.LogInfo("[Fika Module] InitOnPluginEnabled - Subscribing to FikaNetworkManagerCreatedEvent");
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetManagerCreated);
            
            // Initialize SAIN patches for ghost mode (if SAIN is installed)
            InitSAINPatches();
        }
    }
}