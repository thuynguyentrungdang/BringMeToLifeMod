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
        
        public static ModConfig MOD_CONFIG;

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
        public static string ITEM_ID;
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
            MOD_CONFIG = await LoadFromServer();
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

            SELF_REVIVAL_ENABLED = MOD_CONFIG.EnabledSelfRevival;

            REVIVAL_HOLD_DURATION = MOD_CONFIG.SelfRevivalHoldDuration;

            TEAM_REVIVAL_HOLD_DURATION = MOD_CONFIG.TeamRevivalHoldDuration;

            CRITICAL_STATE_TIME = MOD_CONFIG.CriticalStateDuration;

            INVULNERABILITY_DURATION = MOD_CONFIG.InvulnerabilityDuration;

            REVIVAL_COOLDOWN = MOD_CONFIG.RevivalCooldown;

            RESTORE_DESTROYED_BODY_PARTS = MOD_CONFIG.RestoreDestroyedBodyParts;

            RESTORE_DESTROYED_BODY_PARTS_AMOUNT = MOD_CONFIG.RestoreDestroyedBodyPartsPercentage;

            CONTUSION_EFFECT = MOD_CONFIG.ContusionEffect;

            STUN_EFFECT = MOD_CONFIG.StunEffect;

            REVIVAL_RANGE_X = MOD_CONFIG.HitboxXDimension;

            REVIVAL_RANGE_Y = MOD_CONFIG.HitboxYDimension;

            REVIVAL_RANGE_Z = MOD_CONFIG.HitboxZDimension;

            ITEM_ID = MOD_CONFIG.ItemId;

            KEEP_DEFIB_ITEM = MOD_CONFIG.KeepDefibItem;

            #endregion

            #region Hardcore Mode Settings

            GHOST_MODE = MOD_CONFIG.GhostMode;

            GOD_MODE = MOD_CONFIG.GodMode;

            HARDCORE_HEADSHOT_DEFAULT_DEAD = MOD_CONFIG.HeadshotsAreFatal;

            HARDCORE_CHANCE_OF_CRITICAL_STATE = MOD_CONFIG.CriticalStateChance;

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
            catch (Exception)
            {
                NotificationManagerClass.DisplayWarningNotification("Failed to load Bring Me To Life server config - check the server");
                
                return null;
            }
        }
    }
}