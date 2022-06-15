using ReactiveDots;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
    [AlwaysUpdateSystem]
    [ReactiveEventSystem]
    public partial class EventSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactive( Dependency );
            Dependency.Complete();
            Dependency = this.FireEvents( Dependency );
        }
    }
}