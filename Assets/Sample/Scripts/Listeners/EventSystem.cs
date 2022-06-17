// using ReactiveDots;
// using Unity.Entities;
// using UnityEngine;
//
// namespace ReactiveDotsSample
// {
//     [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
//     [AlwaysUpdateSystem]
//     [ReactiveEventSystem]
//     public partial class EventSystem : SystemBase
//     {
//         protected override void OnCreate()
//         {
//             base.OnCreate();
//             InitMethodAttribute.InvokeInitMethodsFor( this );
//         }
//
//         protected override void OnUpdate()
//         {
//             Dependency = UpdateReactive( Dependency );
//             Dependency.Complete();
//             Dependency = FireEvents( Dependency );
//         }
//     }
// }