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
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle>> _reactiveAddMissingReactiveDataMethods = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle>>();
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle>> _reactiveAddMissingReactiveTagMethods = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle>>();
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle>> _reactiveCleanupMethods = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle>>();
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>> _reactiveCoreChecks = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>>();
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>> _reactiveRemovedChecks = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>>();
        private List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>> _eventFires = new List<Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle>>();

        private Unity.Jobs.JobHandle UpdateReactive( Unity.Jobs.JobHandle dependency, 
            EntityCommandBuffer.ParallelWriter ecbForAdded, EntityCommandBuffer.ParallelWriter ecbForMissingTag,
            EntityCommandBuffer.ParallelWriter ecbForCleanup )
        {
            foreach( var update in _reactiveCoreChecks )
                dependency = update( this, dependency );
            foreach( var update in _reactiveAddMissingReactiveDataMethods )
                dependency = update( this, dependency, ecbForAdded );
            foreach( var update in _reactiveAddMissingReactiveTagMethods )
                dependency = update( this, dependency, ecbForMissingTag );
            foreach( var update in _reactiveCleanupMethods )
                dependency = update( this, dependency, ecbForCleanup );
            foreach( var update in _reactiveRemovedChecks )
                dependency = update( this, dependency );
            return dependency;
        }

        private Unity.Jobs.JobHandle FireEvents( Unity.Jobs.JobHandle dependency )
        {
            foreach( var func in _eventFires )
                dependency = func( this, dependency );
            return dependency;
        }

        public void RegisterEventComponent( 
            Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle> addMissingReactiveDataMethod, 
            Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle> addMissingReactiveTagMethod, 
            Func<$$systemName$$, Unity.Jobs.JobHandle, EntityCommandBuffer.ParallelWriter, Unity.Jobs.JobHandle> cleanupMethod, 
            Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle> reactiveCoreCheck,
            Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle> reactiveRemovedCheck,
            Func<$$systemName$$, Unity.Jobs.JobHandle, Unity.Jobs.JobHandle> fireEvents )
        {
            _reactiveAddMissingReactiveDataMethods.Add( addMissingReactiveDataMethod );
            _reactiveAddMissingReactiveTagMethods.Add( addMissingReactiveTagMethod );
            _reactiveCleanupMethods.Add( cleanupMethod );
            _reactiveCoreChecks.Add( reactiveCoreCheck );
            _reactiveRemovedChecks.Add( reactiveRemovedCheck );
            _eventFires.Add( fireEvents );            
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
    public interface IAny$$componentName$$AddedListener
    {
        void OnAny$$componentName$$Added( Entity entity, $$componentNameFull$$ component, World world );
    }

    public class Any$$componentName$$AddedListener : IComponentData
    {
        public IAny$$componentName$$AddedListener Value;
    }    

    // $$componentName$$ Removed
    public interface IAny$$componentName$$RemovedListener
    {
        void OnAny$$componentName$$Removed( Entity entity, World world );
    }

    public class Any$$componentName$$RemovedListener : IComponentData
    {
        public IAny$$componentName$$RemovedListener Value;
    } 

    // $$componentName$$ Changed
    public interface IAny$$componentName$$ChangedListener
    {
        void OnAny$$componentName$$Changed( Entity entity, $$componentNameFull$$ component, World world );
    }

    public class Any$$componentName$$ChangedListener : IComponentData
    {
        public IAny$$componentName$$ChangedListener Value;
    } 
}";
        }

        public static string GetComponentJobsTemplate()
        {
            return @"// Auto Generated Code
$$placeForUsings$$

namespace $$namespace$$
{ $$placeForComponents$$

    public static partial class $$systemName$$_$$componentName$$_ReactiveEvents
    {
$$placeForReactiveComponent$$

        private struct InstanceData
        {
            public EntityQuery reactiveQuery;
            public EntityQuery reactiveForRemovedQuery;
            public EntityQuery anyAddedQuery;
            public EntityQuery anyRemovedQuery;
            public EntityQuery anyChangedQuery;
        }

        private static Dictionary<$$systemNameFull$$, InstanceData> s_instances =
            new Dictionary<$$systemNameFull$$, InstanceData>();

        private static InstanceData GetOrCreateInstanceData( $$systemNameFull$$ sys )
        {
            if ( !s_instances.ContainsKey( sys ) )
                s_instances.Add( sys, CreateInstanceData( sys ) );
            return s_instances[sys];
        }

        private static InstanceData CreateInstanceData( $$systemNameFull$$ sys )
        {
            var data = new InstanceData();
            data.reactiveQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<$$componentNameFull$$>(),
                ComponentType.ReadOnly<$$reactiveComponentNameFull$$>()
            );
            data.reactiveForRemovedQuery = sys.CreateReactiveQuery(
                ComponentType.Exclude<$$componentNameFull$$>(),
                ComponentType.ReadOnly<$$reactiveComponentNameFull$$>()
            );
            data.anyAddedQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<Any$$componentName$$AddedListener>()
            );
            data.anyRemovedQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<Any$$componentName$$RemovedListener>()
            );
            data.anyChangedQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<Any$$componentName$$ChangedListener>()
            );
            return data;
        }

        [ReactiveDots.InitWith(typeof($$systemNameFull$$))]
        private static void Init$$componentName$$Events( $$systemNameFull$$ sys )
        {
            sys.RegisterEventComponent(
                $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveData,
                $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveEntityTag,
                $$systemName$$_$$componentName$$_Reactive.CleanupDestroyedReactiveEntities,
                $$systemName$$_$$componentName$$_Reactive.CheckForChangedOrAdded,
                $$systemName$$_$$componentName$$_Reactive.CheckForRemoved,
                $$systemName$$_$$componentName$$_ReactiveEvents.FireEvents
            );
        }

        // TODO: change to job?
        public static Unity.Jobs.JobHandle FireEvents( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData       = GetOrCreateInstanceData( sys );

            // added & changed:
            var entities = instanceData.reactiveQuery.ToEntityArray( Allocator.Temp );
            Any$$componentName$$AddedListener[]   addedListeners   = null;
            Any$$componentName$$ChangedListener[] changedListeners = null;

            foreach( var e in entities ) {
                var reactiveData = sys.EntityManager.GetComponentData<$$reactiveComponentNameFull$$>( e );
                if( reactiveData.Value.Added ) {
                    if( addedListeners == null )
                        addedListeners = instanceData.anyAddedQuery.ToComponentDataArray<Any$$componentName$$AddedListener>();
                    FireAdded( sys, addedListeners, e, sys.EntityManager.GetComponentData<$$componentNameFull$$>( e ), sys.World );
                }
                if( reactiveData.Value.Changed ) {
                    if( changedListeners == null )
                        changedListeners = instanceData.anyChangedQuery.ToComponentDataArray<Any$$componentName$$ChangedListener>();
                    FireChanged( sys, changedListeners, e, sys.EntityManager.GetComponentData<$$componentNameFull$$>( e ), sys.World );
                }
            }

            entities.Dispose();

            // removed:
            var entitiesForRemoved = instanceData.reactiveForRemovedQuery.ToEntityArray( Allocator.Temp );
            Any$$componentName$$RemovedListener[] removedListeners = null;

            foreach( var e in entitiesForRemoved ) {
                var reactiveData = sys.EntityManager.GetComponentData<$$reactiveComponentNameFull$$>( e );
                if( reactiveData.Value.Removed ) {
                    if( removedListeners == null )
                        removedListeners = instanceData.anyRemovedQuery.ToComponentDataArray<Any$$componentName$$RemovedListener>();
                    FireRemoved( sys, removedListeners, e, sys.World );
                }
            }

            entitiesForRemoved.Dispose();
            return dependency;
        }

        private static void FireAdded( $$systemNameFull$$ sys, Any$$componentName$$AddedListener[] listeners, Unity.Entities.Entity entity, $$componentNameFull$$ component, Unity.Entities.World world )
        {
            foreach( var listener in listeners )
                listener.Value.OnAny$$componentName$$Added( entity, component, world );
        }

        private static void FireRemoved( $$systemNameFull$$ sys, Any$$componentName$$RemovedListener[] listeners, Unity.Entities.Entity entity, Unity.Entities.World world )
        {
            foreach( var listener in listeners )
                listener.Value.OnAny$$componentName$$Removed( entity, world );
        }

        private static void FireChanged( $$systemNameFull$$ sys, Any$$componentName$$ChangedListener[] listeners, Unity.Entities.Entity entity, $$componentNameFull$$ component, Unity.Entities.World world )
        {
            foreach( var listener in listeners )
                listener.Value.OnAny$$componentName$$Changed( entity, component, world );
        }
    }
}";
        }
    }
}