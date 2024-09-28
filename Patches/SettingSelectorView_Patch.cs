using HarmonyLib;
using Kitchen;
using KitchenData;
using TMPro;

namespace KitchenGameplayInfo.Patches
{
    [HarmonyPatch]
    static class SettingSelectorView_Patch
    {
        [HarmonyPatch(typeof(SettingSelectorView), "UpdateData")]
        [HarmonyPostfix]
        static void UpdateData_Postfix(SettingSelectorView.ViewData data, TextMeshPro ___Label)
        {
            if (___Label == null || !GameData.Main.TryGet(data.SettingID, out RestaurantSetting setting))
                return;

            switch (setting.WeatherMode)
            {
                case WeatherMode.Rain:
                    if (!GameData.Main.GlobalLocalisation.PatienceReasonIcons.TryGetValue(PatienceReason.QueueInRain, out string rainIcon))
                        break;
                    ___Label.text = $"{___Label.text} {rainIcon}";
                    break;
                case WeatherMode.Snow:
                    if (!GameData.Main.GlobalLocalisation.PatienceReasonIcons.TryGetValue(PatienceReason.QueueInSnow, out string snowIcon))
                        break;
                    ___Label.text = $"{___Label.text} {snowIcon}";
                    break;
            }
        }
    }
}
