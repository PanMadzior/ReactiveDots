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

        public enum UpdateType
        {
            WithoutEcb,
            WithTempEcb,
            WithExternalEcb
        }

        public UpdateType updateType;

        private EndSimulationEntityCommandBufferSystem _externalCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _externalCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            switch ( updateType ) {
                case UpdateType.WithoutEcb:
                    Dependency = this.UpdateReactiveWithoutEcb( Dependency );
                    break;
                case UpdateType.WithTempEcb:
                    Dependency = this.UpdateReactive( Dependency );
                    break;
                case UpdateType.WithExternalEcb:
                    var ecbForAdded = _externalCommandBufferSystem.CreateCommandBuffer();
                    var ecbForRemoved = _externalCommandBufferSystem.CreateCommandBuffer();
                    Dependency = this.UpdateReactive( Dependency, ecbForAdded, ecbForRemoved );
                    _externalCommandBufferSystem.AddJobHandleForProducer( Dependency );
                    break;
            }

            Entities.ForEach( ( ref Bounces bounces, in MoveDirectionReactive moveDirReactive ) =>
            {
                if ( moveDirReactive.Value.Changed )
                    bounces.Value += 1;
            } ).ScheduleParallel();
        }
    }
}