using Kitchen;
using Kitchen.Layouts;
using KitchenData;
using KitchenMods;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenGameplayInfo
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct CMessIndicator : IComponentData, IModComponent
    {
        public int ApplianceID;
    }

    public class ManageMessLocationIndicators : RestaurantSystem, IModSystem
    {
        EntityQuery MessIndicators;
        EntityQuery TableSets;
        EntityQuery CausesSpills;

        Dictionary<Vector3, int> _indicatorPositions = new Dictionary<Vector3, int>();

        protected override void Initialise()
        {
            base.Initialise();
            MessIndicators = GetEntityQuery(typeof(CMessIndicator), typeof(CPosition));
            TableSets = GetEntityQuery(typeof(CTableSet), typeof(CTablePlace));
            CausesSpills = GetEntityQuery(new QueryHelper()
                .All(typeof(CAppliance), typeof(CPosition), typeof(CCausesSpills))
                .None(typeof(CDestroyApplianceAtDay)));
        }

        protected override void OnUpdate()
        {
            if (!Has<SIsNightTime>())
            {
                if (!MessIndicators.IsEmpty)
                    EntityManager.DestroyEntity(MessIndicators);
                return;
            }

            _indicatorPositions.Clear();
            List<LayoutPosition> searchOffsets = HasStatus(RestaurantStatus.MessRangeIncrease) ? LayoutHelpers.AllNearbyRange2 : LayoutHelpers.AllNearby;

            using NativeArray<CCausesSpills> causesSpills = CausesSpills.ToComponentDataArray<CCausesSpills>(Allocator.Temp);
            using NativeArray<CPosition> causesSpillPositions = CausesSpills.ToComponentDataArray<CPosition>(Allocator.Temp);
            for (int i = 0; i < causesSpills.Length; i++)
            {
                foreach (LayoutPosition offset in searchOffsets)
                {
                    Vector3 candidatePosition = causesSpillPositions[i] + (Vector3)offset;
                    if ((!causesSpills[i].OverwriteOtherMesses && _indicatorPositions.ContainsKey(candidatePosition)) ||
                        !IsMessPossible(causesSpillPositions[i], candidatePosition))
                        continue;

                    _indicatorPositions[candidatePosition] = causesSpills[i].ID;
                }
            }

            if (Require<SKitchenParameters>(out var kitchenParameters))
            {
                int maxGroupSize = kitchenParameters.Parameters.MaximumGroupSize;

                using NativeArray<Entity> tableSetEntities = TableSets.ToEntityArray(Allocator.Temp);
                using NativeArray<CTableSet> tableSets = TableSets.ToComponentDataArray<CTableSet>(Allocator.Temp);

                for (int i = 0; i < tableSetEntities.Length; i++)
                {
                    if (tableSets[i].IsWaitingTable)
                        continue;

                    DynamicBuffer<CTablePlace> tablePlaces = GetBuffer<CTablePlace>(tableSetEntities[i]);
                    int occupiedChairCount = 0;
                    for (int placeIndex = 0; placeIndex < tablePlaces.Length && occupiedChairCount < maxGroupSize; placeIndex++)
                    {
                        Entity chairEntity = tablePlaces[placeIndex].Chair;
                        if (!Require(tablePlaces[placeIndex].Chair, out CApplianceGhostChair ghostChair) ||
                            ghostChair.IsDisabled ||
                            !ghostChair.IsPathable)
                            continue;

                        occupiedChairCount++;

                        if (!Require(chairEntity, out CPosition chairPosition))
                            continue;

                        foreach (LayoutPosition offset in searchOffsets)
                        {
                            Vector3 candidatePosition = chairPosition.Position + (Vector3)offset;
                            if (_indicatorPositions.ContainsKey(candidatePosition) ||
                                !IsMessPossible(chairPosition.Position, candidatePosition))
                                continue;

                            _indicatorPositions[candidatePosition] = Main.DEFAULT_MESS_INDICATOR_ID;
                        }
                    }
                }
            }

            using NativeArray<Entity> messIndicatorEntities = MessIndicators.ToEntityArray(Allocator.Temp);
            using NativeArray<CMessIndicator> messIndicators = MessIndicators.ToComponentDataArray<CMessIndicator>(Allocator.Temp);
            using NativeArray<CPosition> messIndicatorPositions = MessIndicators.ToComponentDataArray<CPosition>(Allocator.Temp);
            for (int i = 0; i < messIndicatorEntities.Length; i++)
            {
                Vector3 position = messIndicatorPositions[i].Position;
                if (!_indicatorPositions.TryGetValue(position, out int messID) ||
                    messIndicators[i].ApplianceID != messID)
                {
                    EntityManager.DestroyEntity(messIndicatorEntities[i]);
                    continue;
                }
                _indicatorPositions.Remove(position);
            }

            foreach (KeyValuePair<Vector3, int> kvp in _indicatorPositions)
            {
                CreateMessIndicator(kvp.Key, kvp.Value);
            }
        }

        private void CreateMessIndicator(Vector3 position, int id)
        {
            Entity indicator = EntityManager.CreateEntity(typeof(CMessIndicator), typeof(CPosition), typeof(CDoNotPersist), typeof(CRequiresView));
            Set(indicator, new CMessIndicator()
            {
                ApplianceID = id
            });
            Set(indicator, new CPosition(position.Rounded()));
            Set(indicator, new CRequiresView()
            {
                Type = Main.MessIndicatorViewType,
                ViewMode = ViewMode.World
            });
        }

        private bool IsMessPossible(Vector3 source, Vector3 target)
        {
            if (target == GetFrontDoor() ||
                GetOccupant(target, OccupancyLayer.Floor) != default ||
                !CanReach(source, target))
                return false;

            Entity occupant = GetOccupant(target, OccupancyLayer.Default);
            if (occupant != default &&
                (!Require(occupant, out CApplianceGhostChair occupantGhostChair) ||
                (!occupantGhostChair.IsDisabled &&
                occupantGhostChair.IsPathable)))
                return false;

            return true;
        }
    }
}
