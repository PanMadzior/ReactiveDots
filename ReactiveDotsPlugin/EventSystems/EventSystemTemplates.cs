namespace ReactiveDotsPlugin
{
    public static class EventSystemTemplates
    {
        public static string GetEventSystemMainTemplate()
        {
            return @"// Auto Generated Code
$$placeForUsings$$

namespace $$namespace$$
{    
    public partial class $$systemName$$
    {
        public  ReactiveEvents Events { private set; get; } = new ReactiveEvents();
        private List<Action<$$systemName$$>> _reactiveAddedRemovedUpdates = new List<Action<$$systemName$$>>();
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>> _reactiveChangedUpdates = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>>();
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>> _eventFires = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>>();

        private Unity.Jobs.JobHandle UpdateReactive( Unity.Jobs.JobHandle dependency )
        {
            foreach( var update in _reactiveAddedRemovedUpdates )
                update( this );
            foreach( var update in _reactiveChangedUpdates )
                dependency = update( this, dependency );
            return dependency;
        }

        private Unity.Jobs.JobHandle FireEvents( Unity.Jobs.JobHandle dependency )
        {
            foreach( var func in _eventFires )
                dependency = func( this, dependency );
            return dependency;
        }

        public EntityQuery CreateReactiveQuery( params ComponentType[] componentTypes )
        {
            return GetEntityQuery( componentTypes );
        }
    }
}
";
        }

        public static string GetComponentInterfacesTemplate()
        {
            return @"// Auto Generated Code
$$placeForUsings$$

namespace $$namespace$$
{
    // $$componentName$$ Added
    public partial class $$systemName$$
    {
        public interface IAny$$componentName$$AddedListener
        {
            void OnAny$$componentName$$Added( Entity entity, $$componentNameFull$$ component, World world );
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
            void OnAny$$componentName$$Changed( Entity entity, $$componentNameFull$$ component, World world );
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
}";
        }

        public static string GetComponentJobsTemplate()
        {
            return @"// Auto Generated Code
$$placeForUsings$$

namespace $$namespace$$
{ $$placeForComponents$$ $$placeForReactiveComponent$$

    // $$componentName$$ Init
    public partial class $$systemName$$
    {
        [ReactiveDots.InitMethod]
        private void Init$$componentName$$Events()
        {
            _reactiveAddedRemovedUpdates.Add( $$systemName$$_$$componentName$$_Reactive.UpdateReactiveAddedRemoved );
            _reactiveChangedUpdates.Add( $$systemName$$_$$componentName$$_Reactive.UpdateReactive );
            _eventFires.Add( $$systemName$$_$$componentName$$_ReactiveEvents.FireEvents );
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
    }
}";
        }
    }
}