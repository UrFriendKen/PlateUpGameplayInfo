using Kitchen;
using KitchenData;
using KitchenMods;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenGameplayInfo
{
    public class SiteApplianceView : UpdatableObjectView<SiteApplianceView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            EntityQuery Views;
            HashSet<int> RenderedAppliances;

            const int ItemSourceReservation = 1270423542;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(typeof(CLayoutInfo), typeof(CLinkedView));
            }

            protected override void OnUpdate()
            {
                if (RenderedAppliances == null)
                {
                    RenderedAppliances = new HashSet<int>();
                    foreach (LayoutProfile layoutProfile in GameData.Main.Get<LayoutProfile>())
                    {
                        if (layoutProfile.RequiredAppliances != default)
                        {
                            foreach (GameDataObject requiredGDO in layoutProfile.RequiredAppliances)
                            {
                                if (!(requiredGDO is Appliance requiredAppliance))
                                    continue;
                                RenderedAppliances.Add(requiredAppliance.ID);
                            }
                        }
                        if (layoutProfile.Table != default)
                            RenderedAppliances.Add(layoutProfile.Table.ID);
                    }

                    if (RenderedAppliances.Contains(ItemSourceReservation))
                        RenderedAppliances.Remove(ItemSourceReservation);

                }

                using NativeArray<Entity> entities = Views.ToEntityArray(Allocator.Temp);
                using NativeArray<CLayoutInfo> layoutInfos = Views.ToComponentDataArray<CLayoutInfo>(Allocator.Temp);
                using NativeArray<CLinkedView> views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                for (int i = 0; i < entities.Length; i++)
                {
                    Entity e = entities[i];
                    CLayoutInfo layoutInfo = layoutInfos[i];
                    CLinkedView view = views[i];

                    if (layoutInfo.Layout == default ||
                        !Require(layoutInfo.Layout, out CBounds cBounds))
                        continue;

                    Bounds bounds = cBounds.Bounds;

                    DynamicBuffer<CLayoutAppliancePlacement> appliancePlacements = GetBuffer<CLayoutAppliancePlacement>(layoutInfo.Layout);

                    List<AppliancePlacementData> placementDatas = new List<AppliancePlacementData>();
                    for (int j = 0; j < appliancePlacements.Length; j++)
                    {
                        CLayoutAppliancePlacement placement = appliancePlacements[j];

                        if (!bounds.Contains(placement.Position) ||
                            !RenderedAppliances.Contains(placement.Appliance))
                            continue;

                        placementDatas.Add(new AppliancePlacementData()
                        {
                            ApplianceID = placement.Appliance,
                            Position = placement.Position,
                            Rotation = placement.Rotation
                        });
                    }

                    SendUpdate(view, new ViewData()
                    {
                        Appliances = placementDatas,
                    });
                }
            }
        }

        [Serializable]
        public struct AppliancePlacementData
        {
            public int ApplianceID;
            public Vector3 Position;
            public Quaternion Rotation;
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public List<AppliancePlacementData> Appliances;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<SiteApplianceView>();

            public bool IsChangedFrom(ViewData check)
            {
                if (Appliances == null || check.Appliances == null)
                    return Appliances != check.Appliances;

                if (Appliances.Count != check.Appliances.Count)
                    return true;

                for (int i = 0; i < Appliances.Count; i++)
                {
                    AppliancePlacementData candidate = Appliances[i];
                    AppliancePlacementData checkCandidate = check.Appliances[i];

                    if (candidate.ApplianceID != checkCandidate.ApplianceID ||
                        candidate.Position != checkCandidate.Position ||
                        candidate.Rotation != checkCandidate.Rotation)
                        return true;
                }
                return false;
            }
        }

        public Transform Container;

        void Update()
        {
            if (Container == null)
                return;

            Container.gameObject.SetActive(Main.PrefManager.Get<ShowCondition>(Main.PREVIEW_APPLIANCES_SHOW_CONDITION_ID) != ShowCondition.Never);
        }

        protected override void UpdateData(ViewData data)
        {
            if (Container == null ||
                data.Appliances == null)
                return;

            int uiLayer = LayerMask.NameToLayer("UI");

            for (int i = 0; i < data.Appliances.Count; i++)
            {
                if (!GameData.Main.TryGet(data.Appliances[i].ApplianceID, out Appliance appliance) ||
                    appliance.Prefab == null)
                    continue;

                Vector3 position = data.Appliances[i].Position;
                Quaternion rotation = data.Appliances[i].Rotation;

                GameObject goInstance = GameObject.Instantiate(appliance.Prefab);
                goInstance.transform.SetParent(Container);
                goInstance.transform.localPosition = position;
                goInstance.transform.localRotation = rotation;
                goInstance.transform.localScale = Vector3.one;

                foreach (Transform childTransform in goInstance.GetComponentsInChildren<Transform>())
                {
                    childTransform.gameObject.layer = uiLayer;
                }
            }
        }
    }
}
