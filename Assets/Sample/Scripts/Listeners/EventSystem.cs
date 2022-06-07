using System.Collections.Generic;
using ReactiveDots;
using ReactiveDotsSample;
using Unity.Entities;

namespace ReactiveDots
{
    [UpdateInGroup( typeof(LateSimulationSystemGroup) )]
    [AlwaysUpdateSystem]
    public partial class EventSystem : SystemBase
    {
        public struct BouncesReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<Bounces> Value;
        }

        public ReactiveEvents Events => _events;
        private ReactiveEvents _events;

        protected override void OnCreate()
        {
            _events = new ReactiveEvents();
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactiveEvents( Dependency );

            Entities.ForEach( ( Entity e, in Bounces bounces, in BouncesReactive reactive ) =>
            {
                if ( reactive.Value.Changed )
                    foreach ( var listener in _events.GetAnyBouncesListeners() )
                        listener.OnAnyBounces( e, bounces, World );
            } ).WithoutBurst().Run();
        }
    }
}