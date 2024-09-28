using HarmonyLib;
using Kitchen;
using KitchenData;
using TMPro;

namespace KitchenGameplayInfo.Patches
{
    [HarmonyPatch]
    static class SiteView_Patch
    {
        [HarmonyPatch(typeof(SiteView), "UpdateData")]
        [HarmonyPrefix]
        static void UpdateData_Prefix(ref bool __state, bool ___IsInitialised)
        {
            __state = ___IsInitialised;
        }

        [HarmonyPatch(typeof(SiteView), "UpdateData")]
        [HarmonyPostfix]
        static void UpdateData_Postfix(SiteView.ViewData view_data, TextMeshPro ___Setting, ref bool __state)
        {
            if (__state || ___Setting == null || !GameData.Main.TryGet(view_data.Setting, out RestaurantSetting setting))
                return;

            switch (setting.WeatherMode)
            {
                case WeatherMode.Rain:
                    if (!GameData.Main.GlobalLocalisation.PatienceReasonIcons.TryGetValue(PatienceReason.QueueInRain, out string rainIcon))
                        break;
                    ___Setting.text = $"{___Setting.text} {rainIcon}";
                    break;
                case WeatherMode.Snow:
                    if (!GameData.Main.GlobalLocalisation.PatienceReasonIcons.TryGetValue(PatienceReason.QueueInSnow, out string snowIcon))
                        break;
                    ___Setting.text = $"{___Setting.text} {snowIcon}";
                    break;
            }
        }
    }
}
