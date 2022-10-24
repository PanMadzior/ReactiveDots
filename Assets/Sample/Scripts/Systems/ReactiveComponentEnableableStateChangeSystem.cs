using Unity.Entities;

namespace ReactiveDotsSample
{
    public partial class ReactiveComponentEnableableStateChangeSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate()
        {
            RequireForUpdate<ReactiveComponentStateChangeRequest>();
            _commandBufferSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb           = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var ecbForRequest = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var newState      = GetSingleton<ReactiveComponentStateChangeRequest>().newState;

            Entities.WithAll<Bounces>()
                .WithEntityQueryOptions( EntityQueryOptions.IgnoreComponentEnabledState )
                .ForEach( ( Entity entity, int entityInQueryIndex ) =>
                {
                    ecb.SetComponentEnabled<Bounces>( entityInQueryIndex, entity, newState );
                } )
                .ScheduleParallel();

            Entities.WithAll<ReactiveComponentStateChangeRequest>()
                .ForEach( ( Entity entity, int entityInQueryIndex ) =>
                {
                    ecbForRequest.DestroyEntity( entityInQueryIndex, entity );
                } )
                .ScheduleParallel();

            _commandBufferSystem.AddJobHandleForProducer( Dependency );
        }
    }
}