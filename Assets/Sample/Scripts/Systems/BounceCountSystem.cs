using ReactiveDots;
using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    [ReactiveSystem( typeof(MoveDirection), typeof(MoveDirectionReactive) )]
    [ReactiveSystem( typeof(Id), typeof(BouncesReactive) )]
    public partial class BounceCountSystem : SystemBase
    {
        public struct MoveDirectionReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<MoveDirection> Value;
        }

        public struct BouncesReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<Id> Value;
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactive( Dependency );

            Entities.ForEach( ( ref Bounces points, in MoveDirectionReactive moveDirReactive,
                in BouncesReactive bouncesReactive ) =>
            {
                if ( !moveDirReactive.Value.Added && moveDirReactive.Value.Changed )
                    points.Value += 1;
                if ( bouncesReactive.Value.Changed )
                    points.Value += 1000;
            } ).ScheduleParallel();
        }
    }
}