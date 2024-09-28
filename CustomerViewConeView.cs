using Kitchen;
using KitchenData;
using KitchenMods;
using MessagePack;
using System.Collections.Generic;
using TwitchLib.Api.Core.Models.Undocumented.ChannelPanels;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenGameplayInfo
{
    public class CustomerViewConeView : UpdatableObjectView<CustomerViewConeView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            EntityQuery Players;
            EntityQuery Groups;

            readonly float InViewLimit = 1f - Mathf.Cos(Mathf.PI / 4f);

            protected override void Initialise()
            {
                base.Initialise();
                Players = GetEntityQuery(typeof(CPlayer), typeof(CPosition));
                Groups = GetEntityQuery(new QueryHelper()
                    .All(typeof(CCustomerGroup), typeof(CGroupMember))
                    .None(typeof(CGroupLeaving), typeof(CGroupStartLeaving)));
            }

            protected override void OnUpdate()
            {
                bool hasShowViewConeStatus = GetOrCreate<SGlobalStatusList>().Has(RestaurantStatus.CustomersLosePatienceInRoom);

                using NativeArray<CPosition> playerPositions = Players.ToComponentDataArray<CPosition>(Allocator.Temp);
                using NativeArray<Entity> groupEntities = Groups.ToEntityArray(Allocator.Temp);

                for (int i = 0; i < groupEntities.Length; i++)
                {
                    Entity groupEntity = groupEntities[i];
                    if (!RequireBuffer(groupEntity, out DynamicBuffer<CGroupMember> members))
                        continue;

                    bool shouldShow = hasShowViewConeStatus &&
                        !(Has<CGroupLeaving>(groupEntity) || Has<CGroupStartLeaving>(groupEntity)) &&
                        (Has<CAtTable>(groupEntity) || Has<CAssignedStand>(groupEntity));

                    for (int j = 0; j < members.Length; j++)
                    {
                        Entity member = members[j];
                        if (!Require(member, out CPosition customerPosition) ||
                            !Require(member, out CLinkedView view))
                            continue;

                        Vector3 customerPositionVec3 = customerPosition.Position;
                        Vector3 customerForward = customerPosition.Forward(1f);
                        int customerRoom = TileManager.GetRoom(customerPositionVec3);

                        for (int k = 0; k < playerPositions.Length; k++)
                        {
                            Vector3 vec = playerPositions[k].Position - customerPositionVec3;
                            bool isActive = TileManager.GetRoom(playerPositions[k]) == customerRoom &&
                                vec.sqrMagnitude < 25f &&
                                Vector3.Dot(vec.normalized, customerForward) > InViewLimit;
                            SendUpdate(view, new ViewData()
                            {
                                ShouldShow = shouldShow,
                                IsActive = isActive
                            });

                        }
                    }
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)]
            public bool ShouldShow;
            [Key(1)]
            public bool IsActive;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<CustomerViewConeView>();

            public bool IsChangedFrom(ViewData check)
            {
                return ShouldShow != check.ShouldShow ||
                    IsActive != check.IsActive;
            }
        }

        private readonly HashSet<string> _supportedShaderNames = new HashSet<string>()
        {
            "Simple Transparent",
            "Simple Flat"
        };

        public MeshRenderer Renderer;
        public Color ActiveColor = new Color(1f, 0f, 0f, 0.05f);
        public Color InactiveColor = new Color(0f, 1f, 0f, 0.05f);
        public bool shouldShow = false;

        void OnDestroy()
        {
            if (Renderer.material != null)
                Destroy(Renderer.material);
        }

        void Update()
        {
            if (Renderer == null)
                return;

            Renderer.gameObject.SetActive(shouldShow && (Main.PrefManager.Get<ShowCondition>(Main.CUSTOMER_VIEW_CONE_SHOW_CONDITION_ID) != ShowCondition.Never));
        }

        protected override void UpdateData(ViewData data)
        {
            shouldShow = data.ShouldShow;
            if (Renderer == null)
                return;

            if (Renderer.material != null && _supportedShaderNames.Contains(Renderer.material.shader.name))
                Renderer.material.SetColor("_Color", data.IsActive ? ActiveColor : InactiveColor);
            else
                Main.LogError("Unsupported Shader");
        }
    }
}
