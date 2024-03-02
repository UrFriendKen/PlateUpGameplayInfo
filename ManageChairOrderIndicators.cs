using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace KitchenGameplayInfo
{
    public class ManageChairOrderIndicators : IndicatorManager, IModSystem
    {
        protected override ViewType ViewType => Main.ChairOrderIndicatorViewType;

        protected override EntityQuery GetSearchQuery()
        {
            return GetEntityQuery(typeof(CApplianceGhostChair), typeof(CPosition));
        }

        protected override bool ShouldHaveIndicator(Entity candidate)
        {
            if (Has<CBeingActedOn>(candidate))
                return false;

            CApplianceGhostChair ghostChair = GetComponent<CApplianceGhostChair>(candidate);
            switch (Main.PrefManager.Get<ShowCondition>(Main.CHAIR_ORDER_SHOW_CONDITION_ID))
            {
                case ShowCondition.Never:
                    return false;
                case ShowCondition.BeingLookedAt:
                    if (!Require(ghostChair.Table, out CPartOfTableSet partOfTableSet) ||
                        !Require(partOfTableSet.TableSet, out DynamicBuffer<CTableSetParts> tableSetParts) ||
                        !Require(partOfTableSet.TableSet, out DynamicBuffer<CTablePlace> tablePlaces))
                        return false;

                    bool hasInteraction = false;

                    bool HasInteraction(Entity candidate)
                    {
                        return Has<CBeingLookedAt>(candidate) ||
                            Has<CBeingActedOn>(candidate) ||
                            Has<CBeingGrabbed>(candidate);

                    }

                    for (int i = 0; i < tableSetParts.Length; i++)
                    {
                        if (HasInteraction(tableSetParts[i].Entity))
                        {
                            hasInteraction = true;
                            break;
                        }
                    }

                    if (!hasInteraction)
                        for (int i = 0; i < tablePlaces.Length; i++)
                        {
                            Entity chairEntity = tablePlaces[i].Chair;
                            if (HasInteraction(chairEntity))
                            {
                                hasInteraction = true;
                                break;
                            }
                        }

                    if (!hasInteraction)
                        return false;

                    break;
                default:
                    break;
            }

            if (ghostChair.IsDisabled ||
                !ghostChair.IsPathable)
                return false;

            

            return true;
        }

        protected override void UpdateIndicator(Entity indicator, Entity source)
        {
            if (!Require(indicator, out CTableSetIndicator tableSetIndicator) ||
                !Require(source, out CApplianceGhostChair sourceGhostChair) ||
                !Require(sourceGhostChair.Table, out CPartOfTableSet partOfTableSet) ||
                !Require(partOfTableSet.TableSet, out DynamicBuffer<CTablePlace> tablePlaces))
            {
                return;
            }

            int order = 0;
            for (int i = 0; i < tablePlaces.Length; i++)
            {
                if (!Require(tablePlaces[i].Chair, out CApplianceGhostChair ghostChair) ||
                    ghostChair.IsDisabled ||
                    !ghostChair.IsPathable)
                    continue;
                
                order++;
                if (tablePlaces[i].Chair == source)
                {
                    tableSetIndicator.Count = order;
                    Set(indicator, tableSetIndicator);
                    break;
                }
            }
        }

        protected override Entity CreateIndicator(Entity source)
        {
            if (!Require(source, out CPosition position))
            {
                return default(Entity);
            }
            Entity entity = base.CreateIndicator(source);
            base.EntityManager.AddComponentData(entity, new CTableSetIndicator
            {
                Count = 0
            });
            base.EntityManager.AddComponentData(entity, new CPosition(position));
            return entity;
        }
    }
}
