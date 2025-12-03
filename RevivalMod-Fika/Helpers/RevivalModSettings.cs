using System;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;
using RevivalMod.Models;
using SPT.Common.Http;

namespace RevivalMod.Helpers
{
    internal class RevivalModSettings
    {
        #region Settings Properties
        
        public static ModConfig ModConfig;

        // Key Bindings
        public static ConfigEntry<KeyCode> SELF_REVIVAL_KEY;
        public static ConfigEntry<KeyCode> GIVE_UP_KEY;

        // Revival Mechanics
        public static bool SELF_REVIVAL_ENABLED;
        public static float REVIVAL_HOLD_DURATION;
        public static float TEAM_REVIVAL_HOLD_DURATION;
        public static float INVULNERABILITY_DURATION;
        public static float REVIVAL_COOLDOWN;
        public static float CRITICAL_STATE_TIME;
        public static bool RESTORE_DESTROYED_BODY_PARTS;
        public static float RESTORE_DESTROYED_BODY_PARTS_AMOUNT;
        public static bool CONTUSION_EFFECT;
        public static bool STUN_EFFECT;
        public static float REVIVAL_RANGE_X;
        public static float REVIVAL_RANGE_Y; 
        public static float REVIVAL_RANGE_Z;
        public static bool KEEP_DEFIB_ITEM;

        // Hardcore Mode
        public static bool GHOST_MODE;
        public static bool GOD_MODE;
        public static bool HARDCORE_HEADSHOT_DEFAULT_DEAD;
        public static float HARDCORE_CHANCE_OF_CRITICAL_STATE;

        // Development
        public static ConfigEntry<bool> TESTING;

        #endregion

        public async static void Init(ConfigFile config)
        {
            ModConfig = await LoadFromServer();
            #region Key Bindings Settings

            SELF_REVIVAL_KEY = config.Bind(
                "1. Key Bindings",
                "Self Revival Key",
                KeyCode.F5,
                "The key to press and hold to revive yourself when in critical state"
            );

            GIVE_UP_KEY = config.Bind(
                "1. Key Bindings",
                "Give Up Key",
                KeyCode.Backspace,
                "Press this key when in critical state to die immediately"
            );

            #endregion

            #region Revival Mechanics Settings

            SELF_REVIVAL_ENABLED = ModConfig.EnabledSelfRevival;

            REVIVAL_HOLD_DURATION = ModConfig.SelfRevivalHoldDuration;

            TEAM_REVIVAL_HOLD_DURATION = ModConfig.TeamRevivalHoldDuration;

            CRITICAL_STATE_TIME = ModConfig.CriticalStateDuration;

            INVULNERABILITY_DURATION = ModConfig.InvulnerabilityDuration;

            REVIVAL_COOLDOWN = ModConfig.RevivalCooldown;

            RESTORE_DESTROYED_BODY_PARTS = ModConfig.RestoreDestroyedBodyParts;

            RESTORE_DESTROYED_BODY_PARTS_AMOUNT = ModConfig.RestoreDestroyedBodyPartsPercentage;

            CONTUSION_EFFECT = ModConfig.ContusionEffect;

            STUN_EFFECT = ModConfig.StunEffect;

            REVIVAL_RANGE_X = ModConfig.HitboxXDimension;

            REVIVAL_RANGE_Y = ModConfig.HitboxYDimension;

            REVIVAL_RANGE_Z = ModConfig.HitboxZDimension;

            KEEP_DEFIB_ITEM = ModConfig.KeepDefibItem;

            #endregion

            #region Hardcore Mode Settings

            GHOST_MODE = ModConfig.GhostMode;

            GOD_MODE = ModConfig.GodMode;

            HARDCORE_HEADSHOT_DEFAULT_DEAD = ModConfig.HeadshotsAreFatal;

            HARDCORE_CHANCE_OF_CRITICAL_STATE = ModConfig.CriticalStateChance;

            #endregion

            #region Development Settings

            TESTING = config.Bind(
                "4. Development",
                "Test Mode",
                false,
                new ConfigDescription("Enables revival without requiring defibrillator item (for testing only)", null, new ConfigurationManagerAttributes { IsAdvanced = true })
            );

            #endregion
        }
        
        private static async Task<ModConfig> LoadFromServer()
        {
            try
            {
                string payload = await RequestHandler.GetJsonAsync("/bringmetolife/load");
                
                return JsonConvert.DeserializeObject<ModConfig>(payload);
            }
            catch (Exception ex)
            {
                NotificationManagerClass.DisplayWarningNotification("Failed to load Bring Me To Life server config - check the server");
                
                return null;
            }
        }
    }
}