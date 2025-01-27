using HarmonyLib;
using Kitchen;
using KitchenData;
using KitchenGameplayInfo.Extensions;
using KitchenGameplayInfo.Utils;
using KitchenMods;
using PreferenceSystem;
using System.Reflection;

// Namespace should have "Kitchen" in the beginning
namespace KitchenGameplayInfo
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = $"IcedMilo.PlateUp.{MOD_NAME}";
        public const string MOD_NAME = "GameplayInfo";
        public const string MOD_VERSION = "0.1.7";

        internal const string CHAIR_ORDER_SHOW_CONDITION_ID = "chairOrderShowCondition";
        internal static readonly ViewType ChairOrderIndicatorViewType = (ViewType)HashUtils.GetID($"{MOD_GUID}:chairOrderIndicator");

        internal const string SHOW_END_OF_DAY_DISH_DETAILS_ID = "showEndOfDayDishDetails";
        internal const string SHOW_DESK_TARGET_INDICATOR_ID = "showDeskTargetIndicator";

        internal const string MESS_INDICATOR_SHOW_CONDITION_ID = "messIndicatorShowCondition";
        internal const string MESS_INDICATOR_USE_MAX_SIZE_ID = "messIndicatorUseMaxSize";
        internal const string CUSTOMER_VIEW_CONE_SHOW_CONDITION_ID = "customerViewConeShowCondition";
        internal const string PREVIEW_APPLIANCES_SHOW_CONDITION_ID = "previewAppliancesShowCondition";
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
                .AddLabel("Show Customer View Cone")
                .AddOption<ShowCondition>(
                    CUSTOMER_VIEW_CONE_SHOW_CONDITION_ID,
                    ShowCondition.Always,
                    new ShowCondition[] { ShowCondition.Never, ShowCondition.Always },
                    new string[] { "Disabled", "Enabled" })
                .AddLabel("Show Appliances in Preview")
                .AddOption<ShowCondition>(
                    PREVIEW_APPLIANCES_SHOW_CONDITION_ID,
                    ShowCondition.Always,
                    new ShowCondition[] { ShowCondition.Never, ShowCondition.Always },
                    new string[] { "Disabled", "Enabled" })
                .AddLabel("Show Desk Target Indicator")
                .AddOption<ShowCondition>(
                    SHOW_DESK_TARGET_INDICATOR_ID,
                    ShowCondition.Always,
                    new ShowCondition[] { ShowCondition.Never, ShowCondition.Always },
                    new string[] { "Disabled", "Enabled" })
                .AddLabel("More End Of Day Popup Details")
                .AddOption<bool>(
                    SHOW_END_OF_DAY_DISH_DETAILS_ID,
                    true,
                    new bool[] { false, true },
                    new string[] { "Hide", "Show" })
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        public void PreInject()
        {
            foreach (Appliance appliance in GameData.Main.Get<Appliance>())
            {
                if (appliance.Prefab == default ||
                    !appliance.HasProperty<CDeskTarget>() ||
                    appliance.Prefab.HasComponent<DeskTargetLineView>())
                    continue;
                appliance.Prefab.AddComponent<DeskTargetLineView>();
            }
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
