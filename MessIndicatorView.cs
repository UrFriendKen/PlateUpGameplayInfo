using Kitchen;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenGameplayInfo
{
    public class MessIndicatorView : UpdatableObjectView<MessIndicatorView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(typeof(CMessIndicator), typeof(CLinkedView));
            }


            protected override void OnUpdate()
            {
                using NativeArray<CMessIndicator> messIndicators = Views.ToComponentDataArray<CMessIndicator>(Allocator.Temp);
                using NativeArray<CLinkedView> views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                for (int i = 0; i < messIndicators.Length; i++)
                {
                    SendUpdate(views[i], new ViewData()
                    {
                        ApplianceID = messIndicators[i].ApplianceID
                    });
                }
            }
        }

        public class ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            public int ApplianceID;

            public bool IsChangedFrom(ViewData check)
            {
                return ApplianceID != check.ApplianceID;
            }
        }

        public Color DefaultMessColor = new Color(0.42f, 0.71f, 0.19f);

        static Dictionary<int, Color> _customMessColors = new Dictionary<int, Color>()
        {
            { 377065033, new Color(0f, 0.8f, 0.8f) }
        };

        public GameObject DefaultMessPrefab;

        GameObject Container;

        GameObject MessGO;

        float lastUpdateTime = 0f;

        public float UpdateShowIndicatorInterval = 1f;

        void Update()
        {
            if (Time.realtimeSinceStartup - lastUpdateTime > UpdateShowIndicatorInterval)
            {
                Container?.SetActive(Main.PrefManager.Get<ShowCondition>(Main.MESS_INDICATOR_SHOW_CONDITION_ID) != ShowCondition.Never);
                lastUpdateTime = Time.realtimeSinceStartup;
            }
        }

        protected override void UpdateData(ViewData data)
        {
            if (MessGO != null)
            {
                GameObject.Destroy(MessGO);
            }

            if (Container == null)
            {
                Container = new GameObject("Container");
                Container.transform.SetParent(GameObject.transform);
                Container.transform.Reset();
            }
            Container.SetActive(Main.PrefManager.Get<ShowCondition>(Main.MESS_INDICATOR_SHOW_CONDITION_ID) != ShowCondition.Never);
            lastUpdateTime = Time.realtimeSinceStartup;

            if (data.ApplianceID == 0)
                data.ApplianceID = Main.DEFAULT_MESS_INDICATOR_ID;

            GameObject prefab = null;
            if (Main.PrefManager.Get<bool>(Main.MESS_INDICATOR_USE_MAX_SIZE_ID))
            {
                int nextMessID = data.ApplianceID;
                HashSet<int> seenMessIDs = new HashSet<int>();
                while (GameData.Main.TryGet(nextMessID, out Appliance appliance) &&
                    appliance.Prefab != default &&
                    !seenMessIDs.Contains(nextMessID))
                {
                    seenMessIDs.Add(nextMessID);
                    prefab = appliance.Prefab;
                    nextMessID = appliance.Properties.Where(x => x.GetType() == typeof(CStackableMess)).Cast<CStackableMess>().FirstOrDefault().NextMess;
                }
            }
            else if (GameData.Main.TryGet(data.ApplianceID, out Appliance appliance, warn_if_fail: true) &&
                appliance.Prefab != default)
            {
                prefab = appliance.Prefab;
            }

            if (prefab == null)
            {
                Main.LogError("DefaultMessPrefab failed!");
                return;
            }

            MessGO = UnityEngine.GameObject.Instantiate(prefab);
            MessGO.transform.SetParent(Container.transform);
            MessGO.transform.Reset();

            Color color = _customMessColors.TryGetValue(data.ApplianceID, out Color customColor) ? customColor : DefaultMessColor;

            foreach (MeshRenderer meshRenderer in MessGO.GetComponentsInChildren<MeshRenderer>())
            {
                try
                {
                    switch (meshRenderer.material?.shader?.name)
                    {
                        case null:
                            Main.LogError($"No mesh renderer material on {gameObject} - {meshRenderer.gameObject}");
                            break;
                        case "Simple Flat":
                            meshRenderer.material.SetColor("_Color0", new Color(0.42f, 0.71f, 0.19f));
                            break;
                        default:
                            meshRenderer.material.color = color;
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    Main.LogError($"{ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
