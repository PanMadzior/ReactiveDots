using ReactiveDots;
using Unity.Collections;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [ReactiveEventFor( typeof(Speed), EventType.All, typeof(CustomEventSystem) )]
    [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
    [ReactiveEventSystem]
    public partial class CustomEventSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            InitWithAttribute.InvokeInitMethodsFor( this );
        }

        protected override void OnUpdate()
        {
            var ecbForAdded      = new EntityCommandBuffer( Allocator.TempJob );
            var ecbForMissingTag = new EntityCommandBuffer( Allocator.TempJob );
            var ecbForCleanup    = new EntityCommandBuffer( Allocator.TempJob );
            Dependency = UpdateReactive( Dependency, ecbForAdded.AsParallelWriter(),
                ecbForMissingTag.AsParallelWriter(), ecbForCleanup.AsParallelWriter() );
            Dependency.Complete();
            ecbForAdded.Playback( EntityManager );
            ecbForAdded.Dispose();
            Dependency = FireEvents( Dependency );
            ecbForMissingTag.Playback( EntityManager );
            ecbForMissingTag.Dispose();
            ecbForCleanup.Playback( EntityManager );
            ecbForCleanup.Dispose();
        }
    }
}