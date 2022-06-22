using ReactiveDots;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [UpdateAfter( typeof(BallMovementSystem) )]
    [ReactiveSystem( typeof(MoveDirection), typeof(MoveDirectionReactive) )]
    public partial class BounceCountSystem : SystemBase
    {
        public struct MoveDirectionReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<MoveDirection> Value;
        }

        public enum UpdateType
        {
            NowWithEntityManager,
            NowWithEcb,
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
                case UpdateType.NowWithEntityManager:
                    Dependency = this.UpdateReactiveNowWithEntityManager( Dependency );
                    break;
                case UpdateType.NowWithEcb:
                    Dependency = this.UpdateReactiveNowWithEcb( Dependency );
                    break;
                case UpdateType.WithExternalEcb:
                    var ecbForAdded   = _externalCommandBufferSystem.CreateCommandBuffer();
                    var ecbForRemoved = _externalCommandBufferSystem.CreateCommandBuffer();
                    Dependency = this.UpdateReactive( Dependency, ecbForAdded, ecbForRemoved );
                    _externalCommandBufferSystem.AddJobHandleForProducer( Dependency );
                    break;
            }

            // Entities.WithNone<MoveDirectionReactive>().ForEach( ( in MoveDirection moveDir ) =>
            //     {
            //         Debug.Log( $"WithNone<> frame {UnityEngine.Time.frameCount}" );
            //     } )
            //     .Run();
            //
            // Entities.ForEach( ( in MoveDirectionReactive moveDirReactive ) =>
            // {
            //     if ( moveDirReactive.Value.Added )
            //         Debug.Log( $".Added frame {UnityEngine.Time.frameCount}" );
            // } ).Run();

            Entities.ForEach( ( ref Bounces bounces, in MoveDirectionReactive moveDirReactive ) =>
            {
                if ( moveDirReactive.Value.Changed )
                    bounces.Value += 1;
            } ).ScheduleParallel();
        }
    }
}