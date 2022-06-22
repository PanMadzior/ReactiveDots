using Unity.Entities;

namespace ReactiveDots
{
    /// <summary>
    /// System which handles event changes by default.
    /// </summary>
    [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
    [AlwaysUpdateSystem]
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
            Dependency = UpdateReactive( Dependency );
            Dependency.Complete();
            Dependency = FireEvents( Dependency );
        }
    }
}