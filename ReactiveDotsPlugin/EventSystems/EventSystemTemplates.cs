namespace ReactiveDotsPlugin
{
    public static class EventSystemTemplates
    {
        public static string GetGlobalTemplate()
        {
            return @"// Auto Generated Code
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using ReactiveDots;

namespace $$namespace$$
{
    public static class $$systemName$$_Reactive
    {
        public static Unity.Jobs.JobHandle UpdateReactive( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        { $$placeForUpdatesAddedRemoved$$ $$placeForUpdatesChanged$$
            return dependency;
        }

        public static Unity.Jobs.JobHandle FireEvents( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        { $$placeForEventFires$$
            return dependency;
        }
    }
$$placeForReactiveComponents$$
$$placeForComponents$$
$$placeForComponentEvents$$
    
    public partial class $$systemName$$
    {
        public ReactiveEvents Events { private set; get; } = new ReactiveEvents();

        public EntityQuery CreateReactiveQuery( params ComponentType[] componentTypes )
        {
            return GetEntityQuery( componentTypes );
        }
    }
}
";
        }

        public static string GetTemplateForEventFire()
        {
            return "            dependency = $$systemName$$_$$componentName$$_ReactiveEvents.FireEvents( sys, dependency );";
        }

        public static string GetTemplateForComponentEvents()
        {
            return @"
    // $$componentName$$ Added
    public partial class $$systemName$$
    {
        public interface IAny$$componentName$$AddedListener
        {
            void OnAny$$componentName$$Added( Entity entity, Bounces bounces, World world );
        }

        public partial class ReactiveEvents
        {
            private List<IAny$$componentName$$AddedListener> _listeners$$componentName$$Added = new List<IAny$$componentName$$AddedListener>();

            public List<IAny$$componentName$$AddedListener> GetAny$$componentName$$AddedListeners()
            {
                return _listeners$$componentName$$Added;
            }

            public void AddAny$$componentName$$AddedListener( IAny$$componentName$$AddedListener listener )
            {
                _listeners$$componentName$$Added.Add( listener );
            }

            public void RemoveAny$$componentName$$AddedListener( IAny$$componentName$$AddedListener listener )
            {
                if ( _listeners$$componentName$$Added.Contains( listener ) )
                    _listeners$$componentName$$Added.Remove( listener );
            }
        }
    }

    // $$componentName$$ Removed
    public partial class $$systemName$$
    {
        public interface IAny$$componentName$$RemovedListener
        {
            void OnAny$$componentName$$Removed( Entity entity, World world );
        }

        public partial class ReactiveEvents
        {
            private List<IAny$$componentName$$RemovedListener> _listeners$$componentName$$Removed = new List<IAny$$componentName$$RemovedListener>();

            public List<IAny$$componentName$$RemovedListener> GetAny$$componentName$$RemovedListeners()
            {
                return _listeners$$componentName$$Removed;
            }

            public void AddAny$$componentName$$RemovedListener( IAny$$componentName$$RemovedListener listener )
            {
                _listeners$$componentName$$Removed.Add( listener );
            }

            public void RemoveAny$$componentName$$RemovedListener( IAny$$componentName$$RemovedListener listener )
            {
                if ( _listeners$$componentName$$Removed.Contains( listener ) )
                    _listeners$$componentName$$Removed.Remove( listener );
            }
        }
    }

    // $$componentName$$ Changed
    public partial class $$systemName$$
    {
        public interface IAny$$componentName$$ChangedListener
        {
            void OnAny$$componentName$$Changed( Entity entity, Bounces bounces, World world );
        }

        public partial class ReactiveEvents
        {
            private List<IAny$$componentName$$ChangedListener> _listeners$$componentName$$Changed = new List<IAny$$componentName$$ChangedListener>();

            public List<IAny$$componentName$$ChangedListener> GetAny$$componentName$$ChangedListeners()
            {
                return _listeners$$componentName$$Changed;
            }

            public void AddAny$$componentName$$ChangedListener( IAny$$componentName$$ChangedListener listener )
            {
                _listeners$$componentName$$Changed.Add( listener );
            }

            public void RemoveAny$$componentName$$ChangedListener( IAny$$componentName$$ChangedListener listener )
            {
                if ( _listeners$$componentName$$Changed.Contains( listener ) )
                    _listeners$$componentName$$Changed.Remove( listener );
            }
        }
    }

    public static partial class $$systemName$$_$$componentName$$_ReactiveEvents
    {
        private struct InstanceData
        {
            public EntityQuery reactiveQuery;
        }

        private static Dictionary<$$systemNameFull$$, InstanceData> Instances =
            new Dictionary<$$systemNameFull$$, InstanceData>();

        private static InstanceData GetOrCreateInstanceData( $$systemNameFull$$ sys )
        {
            if ( !Instances.ContainsKey( sys ) )
                Instances.Add( sys, CreateInstanceData( sys ) );
            return Instances[sys];
        }

        private static InstanceData CreateInstanceData( $$systemNameFull$$ sys )
        {
            var data = new InstanceData();
            data.reactiveQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<$$componentNameFull$$>(),
                ComponentType.ReadOnly<$$reactiveComponentNameFull$$>()
            );
            return data;
        }

        public static Unity.Jobs.JobHandle FireEvents( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            var entities = instanceData.reactiveQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in entities ) {
                var reactiveData = sys.EntityManager.GetComponentData<$$reactiveComponentNameFull$$>( e );
                if( reactiveData.Value.Added )
                    FireAdded( sys, e, sys.EntityManager.GetComponentData<$$componentNameFull$$>( e ), sys.World );
                if( reactiveData.Value.Removed )
                    FireRemoved( sys, e, sys.EntityManager.GetComponentData<$$componentNameFull$$>( e ), sys.World );
                if( reactiveData.Value.Changed )
                    FireChanged( sys, e, sys.EntityManager.GetComponentData<$$componentNameFull$$>( e ), sys.World );
            }

            entities.Dispose();
            return dependency;
        }

        public static void FireAdded( $$systemNameFull$$ sys, Unity.Entities.Entity entity, $$componentNameFull$$ component, Unity.Entities.World world )
        {
            foreach( var listener in sys.Events.GetAny$$componentName$$AddedListeners() )
                listener.OnAny$$componentName$$Added( entity, component, world );
        }

        public static void FireRemoved( $$systemNameFull$$ sys, Unity.Entities.Entity entity, $$componentNameFull$$ component, Unity.Entities.World world )
        {
            foreach( var listener in sys.Events.GetAny$$componentName$$RemovedListeners() )
                listener.OnAny$$componentName$$Removed( entity, world );
        }

        public static void FireChanged( $$systemNameFull$$ sys, Unity.Entities.Entity entity, $$componentNameFull$$ component, Unity.Entities.World world )
        {
            foreach( var listener in sys.Events.GetAny$$componentName$$ChangedListeners() )
                listener.OnAny$$componentName$$Changed( entity, component, world );
        }
    }";
        }
    }
}