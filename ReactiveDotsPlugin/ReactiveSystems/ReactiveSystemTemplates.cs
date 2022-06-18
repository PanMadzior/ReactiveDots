namespace ReactiveDotsPlugin
{
    public static class ReactiveSystemTemplates
    {
        public static string GetTemplateForComponent()
        {
            return @"
    public static class $$systemName$$_$$componentName$$_Reactive
    {
        private struct InstanceData
        {
            public EntityQuery addedQuery;
            public EntityQuery removedQuery;
            public EntityQuery changedQuery;
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
            data.addedQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<$$componentNameFull$$>(),
                ComponentType.Exclude<$$reactiveComponentNameFull$$>()
            );
            data.removedQuery = sys.CreateReactiveQuery(
                ComponentType.Exclude<$$componentNameFull$$>(),
                ComponentType.ReadWrite<$$reactiveComponentNameFull$$>()
            );
            data.changedQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<$$componentNameFull$$>(),
                ComponentType.ReadWrite<$$reactiveComponentNameFull$$>()
            );
            return data;
        }

        public static void UpdateReactiveAddedRemoved( $$systemNameFull$$ sys )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            UpdateAdded( sys, instanceData );
            UpdateRemoved( sys, instanceData );
        }

        public static Unity.Jobs.JobHandle UpdateChanged( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        {
            return GetReactiveUpdateChangedJob( sys, out var query ).ScheduleParallel( query, dependency );
        }

        // TODO: change to job?
        private static void UpdateAdded( $$systemNameFull$$ sys, InstanceData instanceData )
        {
            var addedEntities = instanceData.addedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in addedEntities ) {
                sys.EntityManager.AddComponentData( e, new $$reactiveComponentNameFull$$() {
                    Value = new ComponentReactiveData<$$componentNameFull$$>() {
                        PreviousValue        = sys.EntityManager.GetComponentData<$$componentNameFull$$>( e ),
                        Added                = true,
                        Changed              = false,
                        Removed              = false,
                        _FirstCheckCompleted = false
                    }
                } );
            }

            addedEntities.Dispose();
        }

        // TODO: change to job?
        private static void UpdateRemoved( $$systemNameFull$$ sys, InstanceData instanceData )
        {
            var removedEntities = instanceData.removedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in removedEntities ) {
                var rComp = sys.EntityManager.GetComponentData<$$reactiveComponentNameFull$$>( e );
                var rCompData = rComp.Value;
                if ( rCompData.Removed )
                    sys.EntityManager.RemoveComponent<$$reactiveComponentNameFull$$>( e );
                else {
                    rCompData.Removed = true;
                    rComp.Value       = rCompData;
                    sys.EntityManager.SetComponentData( e, rComp );
                }
            }

            removedEntities.Dispose();
        }

        private static ReactiveUpdateChangedJob GetReactiveUpdateChangedJob( $$systemNameFull$$ sys,
            out EntityQuery query )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.changedQuery;
            return new ReactiveUpdateChangedJob() {
                compHandle  = sys.GetComponentTypeHandle<$$componentNameFull$$>( true ),
                rCompHandle = sys.GetComponentTypeHandle<$$reactiveComponentNameFull$$>( false )
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveUpdateChangedJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<$$componentNameFull$$> compHandle;
            public ComponentTypeHandle<$$reactiveComponentNameFull$$> rCompHandle;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex )
            {
                var compsArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, compHandle );
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr( batchInChunk, rCompHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<$$componentNameFull$$>( compsArrayPtr, i ),
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<$$reactiveComponentNameFull$$>(
                                rCompArrayPtr, i ) 
                        );
                }
            }

            private static void Execute( ref $$componentNameFull$$ comp, ref $$reactiveComponentNameFull$$ rComp )
            {
                rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                    PreviousValue        = comp,
                    Added                = !rComp.Value._FirstCheckCompleted,
                    Changed              = BooleanSimplifier.Any( comp.$$variableNameToCompare$$ != rComp.Value.PreviousValue.$$variableNameToCompare$$ ),
                    Removed              = rComp.Value.Removed,
                    _FirstCheckCompleted = true
                };
            }
        }
    }";
        }
        
        public static string GetGlobalTemplate()
        {
            return @"// Auto Generated Code
$$placeForUsings$$

namespace $$namespace$$
{
    public static class $$systemName$$_Reactive
    {
        public static Unity.Jobs.JobHandle UpdateReactive( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        { $$placeForUpdatesAddedRemoved$$ $$placeForUpdatesChanged$$
            return dependency;
        }
    }
$$placeForComponents$$
    
    public partial class $$systemName$$
    {
        public EntityQuery CreateReactiveQuery( params ComponentType[] componentTypes )
        {
            return GetEntityQuery( componentTypes );
        }
    }
}
";
        }

        public static string GetTemplateForSystemUpdateAddedRemoved()
        {
            return "            $$systemName$$_$$componentName$$_Reactive.UpdateReactiveAddedRemoved( sys );";
        }

        public static string GetTemplateForSystemUpdate()
        {
            return "            dependency = $$systemName$$_$$componentName$$_Reactive.UpdateChanged( sys, dependency );";
        }

        public static string GetTemplateForReactiveComponent()
        {
            return @"        public struct $$componentName$$Reactive : ISystemStateComponentData
        {
            public ComponentReactiveData<$$componentNameFull$$> Value;
        }";
        }
    }
}