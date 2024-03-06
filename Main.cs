using HarmonyLib;
using Kitchen;
using KitchenGameplayInfo.Utils;
using KitchenMods;
using PreferenceSystem;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenGameplayInfo
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = $"IcedMilo.PlateUp.{MOD_NAME}";
        public const string MOD_NAME = "GameplayInfo";
        public const string MOD_VERSION = "0.1.2";

        internal const string CHAIR_ORDER_SHOW_CONDITION_ID = "chairOrderShowCondition";
        internal static readonly ViewType ChairOrderIndicatorViewType = (ViewType)HashUtils.GetID($"{MOD_GUID}:chairOrderIndicator");

        internal const string MESS_INDICATOR_SHOW_CONDITION_ID = "messIndicatorShowCondition";
        internal const string MESS_INDICATOR_USE_MAX_SIZE_ID = "messIndicatorUseMaxSize";
        internal static readonly ViewType MessIndicatorViewType = (ViewType)HashUtils.GetID($"{MOD_GUID}:messIndicator");
        internal const int DEFAULT_MESS_INDICATOR_ID = -1324288299; //147181555;

        internal static PreferenceSystemManager PrefManager;
        
        public Main()
        {
            Harmony harmony = new Harmony(MOD_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
            PrefManager
                .AddLabel("Show Chair Order")
                .AddConditionalBlocker(() => Session.CurrentGameNetworkMode != GameNetworkMode.Host)
                    .AddOption<ShowCondition>(
                        CHAIR_ORDER_SHOW_CONDITION_ID,
                        ShowCondition.Always,
                        new ShowCondition[] { ShowCondition.Never, ShowCondition.BeingLookedAt, ShowCondition.Always },
                        new string[] { "Never", "When Being Looked At", "Always" })
                .ConditionalBlockerDone()
                .AddConditionalBlocker(() => Session.CurrentGameNetworkMode == GameNetworkMode.Host)
                    .AddInfo("Follows host preference")
                .ConditionalBlockerDone()
                .AddLabel("Show Mess Indicators")
                .AddOption<ShowCondition>(
                    MESS_INDICATOR_SHOW_CONDITION_ID,
                    ShowCondition.Always,
                    new ShowCondition[] { ShowCondition.Never, ShowCondition.Always },
                    new string[] { "Disabled", "Enabled" })
                .AddLabel("Mess Size Display")
                .AddOption<bool>(
                    MESS_INDICATOR_USE_MAX_SIZE_ID,
                    false,
                    new bool[] { false, true },
                    new string[] { "Minimum Size", "Maximum Size" })
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        public void PreInject()
        {
        }

        public void PostInject()
        {
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
