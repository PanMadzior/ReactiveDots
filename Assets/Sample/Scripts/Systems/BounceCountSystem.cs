using ReactiveDots;
using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    [ReactiveSystem( typeof(MoveDirection), typeof(MoveDirectionReactive) )]
    public partial class BounceCountSystem : SystemBase
    {
        public struct MoveDirectionReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<MoveDirection> Value;
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactive( Dependency );

            Entities.ForEach( ( ref Bounces points, in MoveDirectionReactive moveDirReactive ) =>
            {
                if ( !moveDirReactive.Value.Added && moveDirReactive.Value.Changed )
                    points.Value += 1;
            } ).ScheduleParallel();
        }
    }
}