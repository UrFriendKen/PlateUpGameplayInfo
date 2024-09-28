using HarmonyLib;
using Kitchen;
using KitchenData;
using KitchenGameplayInfo.Utils;
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

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ViewType view_type, ref GameObject __result)
        {
            if (view_type == ViewType.Customer || view_type == ViewType.CustomerCat)
            {
                if (__result.HasComponent<CustomerViewConeView>())
                    return;

                CustomerViewConeView coneView = __result.AddComponent<CustomerViewConeView>();

                GameObject coneGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coneGO.transform.SetParent(__result.transform);
                coneGO.transform.Reset();
                coneGO.transform.localPosition = Vector3.up * 0.03f;
                coneGO.transform.localScale = new Vector3(5f, 1f, 5f);

                foreach (Collider collider in coneGO.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = false;
                }

                coneView.Renderer = coneGO.GetComponentInChildren<MeshRenderer>();
                coneView.Renderer.material = new Material(Shader.Find("Simple Transparent"));

                coneView.Renderer.gameObject.SetActive(false);

                MeshFilter meshFilter = coneGO.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null)
                {
                    float halfAngle = Mathf.Acos(1f - Mathf.Cos(Mathf.PI / 4f));

                    Mesh mesh = new Mesh();
                    List<Vector3> vertices = new List<Vector3>() { Vector3.zero, new Vector3(Mathf.Cos(Mathf.PI / 2f - halfAngle), 0f, Mathf.Sin(Mathf.PI / 2f - halfAngle)) };
                    int resolution = 20;
                    for (int i = 0; i < resolution / 2; i++)
                    {
                        float angle = 2 * Mathf.PI * i / resolution;
                        if (angle <= Mathf.PI / 2f - halfAngle ||
                            angle >= Mathf.PI / 2f + halfAngle)
                            continue;

                        float x = Mathf.Cos(angle);
                        float z = Mathf.Sin(angle);
                        vertices.Add(new Vector3(x, 0f, z));
                    }
                    vertices.Add(new Vector3(Mathf.Cos(Mathf.PI / 2f + halfAngle), 0f, Mathf.Sin(Mathf.PI / 2f + halfAngle)));
                    mesh.vertices = vertices.ToArray();

                    List<int> triangles = new List<int>();
                    for (int i = mesh.vertices.Length - 1; i > 1; i--)
                    {
                        triangles.Add(0);
                        triangles.Add(i);
                        triangles.Add(i - 1);
                    }
                    mesh.SetTriangles(triangles, 0);
                    mesh.RecalculateNormals();

                    meshFilter.mesh = mesh;
                }
                return;
            }

            if (view_type == ViewType.LayoutInfo)
            {
                if (__result.HasComponent<SiteApplianceView>())
                    return;

                SiteApplianceView siteApplianceView = __result.AddComponent<SiteApplianceView>();

                Transform container = __result.transform.Find("Container/Body/Layout Container/Container");
                GameObject applianceContainer = null;
                if (container != null)
                {
                    applianceContainer = new GameObject("Appliance Container");
                    applianceContainer.transform.SetParent(container, false);
                    applianceContainer.transform.Reset();
                    applianceContainer.SetActive(false);
                }
                siteApplianceView.Container = applianceContainer.transform;

                return;
            }
        }
    }
}
