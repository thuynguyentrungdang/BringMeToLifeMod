using Comfort.Common;
using EFT.Communications;
using EFT.UI;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using RevivalMod.Components;
using RevivalMod.Features;
using RevivalMod.FikaModule.Packets;
using RevivalMod.Helpers;
using System;
using UnityEngine;
using TMPro;

namespace RevivalMod.FikaModule.Common
{
    internal class FikaMethods
    {
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

        private static void OnPlayerPositionPacketReceived(PlayerPositionPacket packet, NetPeer peer)
        {
            if (FikaBackendUtils.IsServer && FikaBackendUtils.IsHeadless)
            {
                SendPlayerPositionPacket(packet.playerId, packet.timeOfDeath, packet.position);
            }
            else
            {
                RMSession.AddToCriticalPlayers(packet.playerId, packet.position);
            }
        }
        
        private static void OnRemovePlayerFromCriticalPlayersListPacketReceived(RemovePlayerFromCriticalPlayersListPacket packet, NetPeer peer)
        {
            if (FikaBackendUtils.IsServer && FikaBackendUtils.IsHeadless)
            {
                SendRemovePlayerFromCriticalPlayersListPacket(packet.playerId);
            }
            else
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
            if (FikaBackendUtils.IsServer && FikaBackendUtils.IsHeadless)
            {
                SendReviveMePacket(packet.reviveeId, packet.reviverId);
            }
            else
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
            if (FikaBackendUtils.IsServer && FikaBackendUtils.IsHeadless)
            {
                SendReviveSucceedPacket(packet.reviverId, peer);
            }
            else
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
            if (FikaBackendUtils.IsServer && FikaBackendUtils.IsHeadless)
            {
                SendReviveStartedPacket(packet.reviveeId, packet.reviverId);
            }
            else
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
            if (FikaBackendUtils.IsServer && FikaBackendUtils.IsHeadless)
            {
                SendReviveCanceledPacket(packet.reviveeId, packet.reviverId);
            }
            else
            {
                if (FikaBackendUtils.Profile.ProfileId != packet.reviveeId)
                    return;
                    
                Plugin.LogSource.LogDebug("ReviveCanceled packet received");

                Singleton<GameUI>.Instance.BattleUiPanelExtraction.Close();

                RevivalFeatures.criticalStateMainTimer.StartCountdown(RevivalFeatures._playerList[packet.reviveeId].CriticalTimer,
                                                                    "Critical State Timer", TimerPosition.MiddleCenter);
            }
        }

        public static void OnFikaNetManagerCreated(FikaNetworkManagerCreatedEvent managerCreatedEvent)
        {
            managerCreatedEvent.Manager.RegisterPacket<PlayerPositionPacket, NetPeer>(OnPlayerPositionPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<RemovePlayerFromCriticalPlayersListPacket, NetPeer>(OnRemovePlayerFromCriticalPlayersListPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<ReviveMePacket, NetPeer>(OnReviveMePacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<RevivedPacket, NetPeer>(OnReviveSucceedPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<ReviveStartedPacket, NetPeer>(OnReviveStartedPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<ReviveCanceledPacket, NetPeer>(OnReviveCanceledPacketReceived);
        }
        
        public static void InitOnPluginEnabled()
        {
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetManagerCreated);
        }
    }
}