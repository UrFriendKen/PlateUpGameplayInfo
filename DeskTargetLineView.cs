using Kitchen;
using Kitchen.Layouts;
using KitchenMods;
using MessagePack;
using Shapes;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenGameplayInfo
{
    public class DeskTargetLineView : UpdatableObjectView<DeskTargetLineView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            EntityQuery Desks;

            protected override void Initialise()
            {
                base.Initialise();
                Desks = GetEntityQuery(typeof(CDeskTarget), typeof(CPosition), typeof(CLinkedView));
            }

            protected override void OnUpdate()
            {
                using NativeArray<CDeskTarget> targets = Desks.ToComponentDataArray<CDeskTarget>(Allocator.Temp);
                using NativeArray<CPosition> positions = Desks.ToComponentDataArray<CPosition>(Allocator.Temp);
                using NativeArray<CLinkedView> views = Desks.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                for (int i = 0; i < views.Length; i++)
                {
                    CLinkedView view = views[i];
                    CDeskTarget target = targets[i];

                    Vector3 vecOffset = Vector3.zero;

                    if (target.Target != default)
                    {
                        CPosition position = positions[i];
                        foreach (LayoutPosition offset in LayoutHelpers.AllNearby)
                        {
                            vecOffset = (Vector3)offset;

                            if (TileManager.GetOccupant(position + vecOffset) != target.Target)
                                continue;

                            vecOffset = (Vector3)offset;
                            break;
                        }
                    }

                    SendUpdate(view, new ViewData()
                    {
                        OffsetX = vecOffset.x,
                        OffsetZ = vecOffset.z
                    });
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public float OffsetX;
            [Key(1)] public float OffsetZ;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<DeskTargetLineView>();

            public bool IsChangedFrom(ViewData check)
            {
                return OffsetX != check.OffsetX ||
                    OffsetZ != check.OffsetZ;
            }
        }

        GameObject _lineObject;

        protected override void UpdateData(ViewData data)
        {
            if (_lineObject != null)
                DestroyImmediate(_lineObject);

            if (Main.PrefManager.Get<ShowCondition>(Main.SHOW_DESK_TARGET_INDICATOR_ID) == ShowCondition.Never)
                return;

            if (data.OffsetX == default &&
                data.OffsetZ == default)
                return;

            _lineObject = new GameObject("Desk Target Line");
            _lineObject.transform.SetParent(transform);
            _lineObject.transform.Reset();
            //_lineObject.layer = LayerMask.NameToLayer("UI");

            Line line = _lineObject.AddComponent<Line>();
            line.Start = Vector3.up;
            line.End = line.Start + new Vector3(data.OffsetX, 0f, data.OffsetZ);

            line.Color = Color.red;
            line.Thickness = 0.05f;
        }
    }
}
