using Unity.Collections;
using Unity.Entities;

namespace ReactiveDots
{
    /// <summary>
    /// System which handles event changes by default.
    /// </summary>
    [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
    [ReactiveEventSystem]
    public partial class DefaultEventSystem : SystemBase
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