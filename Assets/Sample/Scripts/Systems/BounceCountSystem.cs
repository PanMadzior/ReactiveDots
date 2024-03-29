﻿using ReactiveDots;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [UpdateAfter( typeof(BallMovementSystem) )]
    [ReactiveSystem( typeof(MoveDirection), typeof(MoveDirectionReactive) )]
    public partial class BounceCountSystem : SystemBase
    {
        public struct MoveDirectionReactive : ICleanupComponentData
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
            _externalCommandBufferSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            Entities.ForEach( ( ref Bounces bounces, in MoveDirectionReactive moveDirReactive ) =>
            {
                if ( moveDirReactive.Value.Changed )
                    bounces.Value += 1;
            } ).ScheduleParallel();
            
            switch ( updateType ) {
                case UpdateType.NowWithEntityManager:
                    Dependency = this.UpdateReactiveNowWithEntityManager( Dependency );
                    break;
                case UpdateType.NowWithEcb:
                    Dependency = this.UpdateReactiveNowWithEcb( Dependency );
                    break;
                case UpdateType.WithExternalEcb:
                    var ecbForAdded      = _externalCommandBufferSystem.CreateCommandBuffer();
                    var ecbForMissingTag = _externalCommandBufferSystem.CreateCommandBuffer();
                    var ecbForCleanup    = _externalCommandBufferSystem.CreateCommandBuffer();
                    Dependency = this.UpdateReactive( Dependency, ecbForAdded, ecbForMissingTag, ecbForCleanup );
                    _externalCommandBufferSystem.AddJobHandleForProducer( Dependency );
                    break;
            }
        }
    }
}