// using Unity.Entities;
//
// namespace ReactiveDots
// {
//     [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
//     [AlwaysUpdateSystem]
//     [ReactiveEventSystem]
//     public partial class DefaultEventSystem : SystemBase
//     {
//         protected override void OnUpdate()
//         {
//             Dependency = this.UpdateReactive( Dependency );
//             Dependency = this.FireEvents( Dependency );
//         }
//     }
// }