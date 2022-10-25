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
            var ecb = new EntityCommandBuffer( Allocator.TempJob );
            Dependency = UpdateReactive( Dependency, ecb.AsParallelWriter() );
            Dependency.Complete();
            ecb.Playback( EntityManager );
            ecb.Dispose();
            Dependency = FireEvents( Dependency );
        }
    }
}