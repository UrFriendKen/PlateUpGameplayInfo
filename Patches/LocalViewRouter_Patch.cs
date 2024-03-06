using HarmonyLib;
using Kitchen;
using KitchenData;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KitchenGameplayInfo.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        static MethodInfo m_GetPrefab = typeof(LocalViewRouter).GetMethod("GetPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

        static GameObject _container = null;

        static Dictionary<ViewType, GameObject> _prefabs = new Dictionary<ViewType, GameObject>();

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPrefix]
        static bool GetPrefab_Prefix(ViewType view_type, ref LocalViewRouter __instance, ref GameObject __result)
        {
            if (_container == null)
            {
                _container = new GameObject("GameplayInfo Container");
                _container.SetActive(false);
                _prefabs = new Dictionary<ViewType, GameObject>();
            }

            if (_prefabs.TryGetValue(view_type, out GameObject viewPrefab))
            {
                __result = viewPrefab;
                return false;
            }    

            if (view_type == Main.ChairOrderIndicatorViewType)
            {
                GameObject tableIndicatorPrefab = (GameObject)m_GetPrefab?.Invoke(__instance, new object[] { ViewType.TableIndicator });
                if (tableIndicatorPrefab != null)
                {
                    viewPrefab = GameObject.Instantiate(tableIndicatorPrefab);
                    viewPrefab.name = "Chair Order Indicator";

                    Transform seats = viewPrefab.transform.Find("Container")?.Find("Seats");
                    if (seats)
                        seats.transform.localScale = Vector3.one * 0.8f;
                }
            }

            if (view_type == Main.MessIndicatorViewType)
            {
                viewPrefab = new GameObject("Mess Indicator");
                MessIndicatorView messIndicatorView = viewPrefab.AddComponent<MessIndicatorView>();
                messIndicatorView.DefaultMessPrefab = GameData.Main.TryGet(Main.DEFAULT_MESS_INDICATOR_ID, out Appliance messCustomer3, warn_if_fail: true) ? messCustomer3.Prefab : null;
            }

            if (viewPrefab != null)
            {
                viewPrefab.transform.SetParent(_container.transform);
                viewPrefab.transform.Reset();

                _prefabs.Add(view_type, viewPrefab);
                __result = viewPrefab;
                return false;
            }
            return true;
        }
    }
}
