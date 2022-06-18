using ReactiveDots;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [ReactiveEventFor( typeof(ReactiveDotsSample.Speed), EventType.All, typeof(ReactiveDotsSample.CustomEventSystem) )]
    [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
    [AlwaysUpdateSystem]
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
            Dependency = UpdateReactive( Dependency );
            Dependency.Complete();
            Dependency = FireEvents( Dependency );
        }
    }
}