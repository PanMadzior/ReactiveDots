using ReactiveDots;
using Unity.Entities;

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

            Entities.ForEach( ( ref Bounces bounces, in MoveDirectionReactive moveDirReactive ) =>
            {
                if ( moveDirReactive.Value.Changed )
                    bounces.Value += 1;
            } ).ScheduleParallel();
        }
    }
}