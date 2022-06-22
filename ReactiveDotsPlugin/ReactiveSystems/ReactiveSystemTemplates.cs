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
            public EntityCommandBufferSystem ecbSystem;
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
                        Removed              = false
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

        public static Unity.Jobs.JobHandle UpdateAdded( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency, EntityCommandBuffer.ParallelWriter ecb )
        {
            return GetReactiveUpdateAddedJob( sys, out var query, ecb ).ScheduleParallel( query, dependency );
        }

        private static ReactiveUpdateAddedJob GetReactiveUpdateAddedJob( $$systemNameFull$$ sys,
            out EntityQuery query, EntityCommandBuffer.ParallelWriter ecb )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.addedQuery;
            return new ReactiveUpdateAddedJob() {
                compHandle  = sys.GetComponentTypeHandle<$$componentNameFull$$>( true ),
                entityHandle = sys.GetEntityTypeHandle(),
                ecb = ecb
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveUpdateAddedJob : IJobEntityBatchWithIndex
        {
            [ReadOnly]
            public ComponentTypeHandle<$$componentNameFull$$> compHandle;
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery )
            {
                var entities      = batchInChunk.GetNativeArray( entityHandle );
                var compsArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, compHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        entities[i],
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<$$componentNameFull$$>( compsArrayPtr, i ),
                        ecb,
                        indexOfFirstEntityInQuery );
                }
            }

            private static void Execute( Entity entity, ref $$componentNameFull$$ comp, EntityCommandBuffer.ParallelWriter ecb, int indexOfFirstEntityInQuery )
            {
                var rComp = new $$reactiveComponentNameFull$$() {
                   Value = new ComponentReactiveData<$$componentNameFull$$>() {
                        PreviousValue        = comp,
                        Added                = true,
                        Changed              = false,
                        Removed              = false
                    }
                };
                ecb.AddComponent( indexOfFirstEntityInQuery, entity, rComp );
            }
        }

        public static Unity.Jobs.JobHandle UpdateRemoved( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency, EntityCommandBuffer.ParallelWriter ecb )
        {
            return GetReactiveUpdateRemovedJob( sys, out var query, ecb ).ScheduleParallel( query, dependency );
        }

        private static ReactiveUpdateRemovedJob GetReactiveUpdateRemovedJob( $$systemNameFull$$ sys,
            out EntityQuery query, EntityCommandBuffer.ParallelWriter ecb )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.removedQuery;
            return new ReactiveUpdateRemovedJob() {
                rCompHandle = sys.GetComponentTypeHandle<$$reactiveComponentNameFull$$>( false ),
                entityHandle = sys.GetEntityTypeHandle(),
                ecb = ecb
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveUpdateRemovedJob : IJobEntityBatchWithIndex
        {
            public ComponentTypeHandle<$$reactiveComponentNameFull$$> rCompHandle;
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery )
            {
                var entities      = batchInChunk.GetNativeArray( entityHandle );
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr( batchInChunk, rCompHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        entities[i],
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<$$reactiveComponentNameFull$$>( rCompArrayPtr, i ),
                        ecb,
                        indexOfFirstEntityInQuery );
                }
            }

            private static void Execute( Entity entity, ref $$reactiveComponentNameFull$$ rComp, EntityCommandBuffer.ParallelWriter ecb, int indexOfFirstEntityInQuery )
            {
                var rCompData = rComp.Value;
                if ( rCompData.Removed )
                    ecb.RemoveComponent<$$reactiveComponentNameFull$$>( indexOfFirstEntityInQuery, entity );
                else {
                    rCompData.Removed = true;
                    rComp.Value       = rCompData;
                    ecb.SetComponent( indexOfFirstEntityInQuery, entity, rComp );
                }
            }
        }

        public static Unity.Jobs.JobHandle UpdateChanged( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        {
            return GetReactiveUpdateChangedJob( sys, out var query ).ScheduleParallel( query, dependency );
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
                    Added                = false,
                    Changed              = BooleanSimplifier.Any( comp.$$variableNameToCompare$$ != rComp.Value.PreviousValue.$$variableNameToCompare$$ ),
                    Removed              = rComp.Value.Removed
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
        /// <summary>
        /// Updates reactive components on the main thread with EntityManager.
        /// This method is NOT recommended as it makes a sync point.
        /// </summary>
        public static Unity.Jobs.JobHandle UpdateReactiveNowWithEntityManager( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        { $$placeForUpdatesChanged$$ $$placeForUpdatesAddedRemoved_WithoutEcb$$
            return dependency;
        }

        /// <summary>
        /// Updates reactive components in parallel jobs with a temporary EntityCommandBuffer.
        /// This method is NOT recommended as it makes a sync point.
        /// </summary>
        public static Unity.Jobs.JobHandle UpdateReactiveNowWithEcb( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        { $$placeForUpdatesChanged$$ $$placeForUpdatesAddedRemoved_WithTempEcb$$
            return dependency;
        }

        /// <summary>
        /// Updates reactive components in parallel jobs with EntityCommandBuffer from EndSimulationEntityCommandBufferSystem.
        /// Note that .Added and .Removed will not be updated until an ECB playback and will be available with one frame delay.
        /// If you need to know if component was added without a one frame delay, use Entities.WithNone{ReactiveComp}.
        /// If you need to know if component was removed without a one frame delay, use Entities.WithNone{Comp}.WithAll{ReactiveComp}.
        /// </summary>
        public static Unity.Jobs.JobHandle UpdateReactive( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        {
            // TODO: cache it maybe?
            var defaultEcb = sys.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            return UpdateReactive( sys, dependency, defaultEcb );
        }

        /// <summary>
        /// Updates reactive components in parallel jobs with EntityCommandBuffer from a passed EntityCommandBufferSystem.
        /// Note that .Added and .Removed will not be updated until an ECB playback and will be available with one frame delay.
        /// If you need to know if component was added without a one frame delay, use Entities.WithNone{ReactiveComp}.
        /// If you need to know if component was removed without a one frame delay, use Entities.WithNone{Comp}.WithAll{ReactiveComp}.
        /// </summary>
        public static Unity.Jobs.JobHandle UpdateReactive( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency, EntityCommandBufferSystem ecbSystem )
        {
            var ecbForAdded   = ecbSystem.CreateCommandBuffer();
            var ecbForRemoved = ecbSystem.CreateCommandBuffer();
            dependency = UpdateReactive( sys, dependency, ecbForAdded, ecbForRemoved );
            ecbSystem.AddJobHandleForProducer( dependency );
            return dependency;
        }

        /// <summary>
        /// Updates reactive components in parallel jobs with passed EntityCommandBuffers.
        /// Note that .Added and .Removed will not be updated until an ECB playback and might be available with one frame delay.
        /// If you need to know if component was added without a one frame delay, use Entities.WithNone{ReactiveComp}.
        /// If you need to know if component was removed without a one frame delay, use Entities.WithNone{Comp}.WithAll{ReactiveComp}.
        /// </summary>
        public static Unity.Jobs.JobHandle UpdateReactive( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency, EntityCommandBuffer ecbForAdded, EntityCommandBuffer ecbForRemoved )
        {
$$placeForUpdatesChanged$$
            var ecbWriterForAdded = ecbForAdded.AsParallelWriter();
            var ecbWriterForRemoved = ecbForRemoved.AsParallelWriter();
$$placeForUpdatesAddedRemoved_WithExternalEcb$$
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

        public static string GetTemplateForSystemUpdateAddedRemoved_WithoutEcb()
        {
            return "            $$systemName$$_$$componentName$$_Reactive.UpdateReactiveAddedRemoved( sys );";
        }

        public static string GetTemplateForSystemUpdateAddedRemoved_WithTempEcb()
        {
            return @"
            var $$systemName$$_$$componentName$$_ecbForAdded = new EntityCommandBuffer( Allocator.TempJob );
            var $$systemName$$_$$componentName$$_ecbWriterForAdded = 
                $$systemName$$_$$componentName$$_ecbForAdded.AsParallelWriter();
            var $$systemName$$_$$componentName$$_ecbForRemoved = new EntityCommandBuffer( Allocator.TempJob );
            var $$systemName$$_$$componentName$$_ecbWriterForRemoved = 
                $$systemName$$_$$componentName$$_ecbForAdded.AsParallelWriter();

            dependency = $$systemName$$_$$componentName$$_Reactive.UpdateAdded( sys, dependency, $$systemName$$_$$componentName$$_ecbWriterForAdded );
            dependency = $$systemName$$_$$componentName$$_Reactive.UpdateRemoved( sys, dependency, $$systemName$$_$$componentName$$_ecbWriterForRemoved );
            dependency.Complete();

            $$systemName$$_$$componentName$$_ecbForAdded.Playback( sys.EntityManager );
            $$systemName$$_$$componentName$$_ecbForAdded.Dispose();
            $$systemName$$_$$componentName$$_ecbForRemoved.Playback( sys.EntityManager );
            $$systemName$$_$$componentName$$_ecbForRemoved.Dispose();";
        }

        public static string GetTemplateForSystemUpdateAddedRemoved_WithExternalEcb()
        {
            return @"
            dependency = $$systemName$$_$$componentName$$_Reactive.UpdateAdded( sys, dependency, ecbWriterForAdded );
            dependency = $$systemName$$_$$componentName$$_Reactive.UpdateRemoved( sys, dependency, ecbWriterForRemoved );";
        }

        public static string GetTemplateForSystemUpdate()
        {
            return
                "            dependency = $$systemName$$_$$componentName$$_Reactive.UpdateChanged( sys, dependency );";
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