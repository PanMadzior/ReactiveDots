using Unity.Entities;

namespace ReactiveDots
{
    [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
    [AlwaysUpdateSystem]
    [ReactiveEventSystem]
    public partial class DefaultEventSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            InitMethodAttribute.InvokeInitMethodsFor( this );
        }

        protected override void OnUpdate()
        {
            Dependency = UpdateReactive( Dependency );
            Dependency.Complete();
            Dependency = FireEvents( Dependency );
        }
    }
}