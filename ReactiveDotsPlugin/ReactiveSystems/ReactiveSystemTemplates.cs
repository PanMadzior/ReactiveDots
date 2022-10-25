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
            public EntityQuery reactiveEntityTagQuery;
            public EntityQuery addedQuery;
            public EntityQuery removedQuery;
            public EntityQuery changedQuery;
            public EntityQuery cleanupQuery;
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
            data.reactiveEntityTagQuery = sys.CreateReactiveQuery(
                //ComponentType.ReadOnly<$$reactiveComponentNameFull$$>(),
                ComponentType.ReadOnly<$$componentNameFull$$>(),
                ComponentType.Exclude<ReactiveEntityTag>()
            );
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
            data.cleanupQuery = sys.CreateReactiveQuery(
                ComponentType.ReadOnly<$$reactiveComponentNameFull$$>(),
                ComponentType.Exclude<$$componentNameFull$$>(),
                ComponentType.Exclude<ReactiveEntityTag>()
            );
            return data;
        }

        public static Unity.Jobs.JobHandle AddMissingReactiveDataWithEntityManager( $$systemNameFull$$ sys, 
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            if( !instanceData.addedQuery.IsEmpty ) {
                dependency.Complete();
                var addedEntities = instanceData.addedQuery.ToEntityArray( Allocator.Temp );
                foreach ( var e in addedEntities ) {
                    sys.EntityManager.AddComponentData( e, new $$reactiveComponentNameFull$$() {
                        Value = new ComponentReactiveData<$$componentNameFull$$>() {
                            PreviousValue = sys.EntityManager.GetComponentData<$$componentNameFull$$>( e ),
                            Added         = true,
                            Changed       = false,
                            Removed       = false,
                            _AddedCheck   = true
                        }
                    } );
                }
                addedEntities.Dispose();
            }
            return dependency;
        }

        public static Unity.Jobs.JobHandle AddMissingReactiveData( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency, EntityCommandBuffer.ParallelWriter ecb )
        {
            return GetReactiveDataAddJob( sys, out var query, ecb ).ScheduleParallel( query, dependency );
        }

        private static ReactiveDataAddJob GetReactiveDataAddJob( $$systemNameFull$$ sys,
            out EntityQuery query, EntityCommandBuffer.ParallelWriter ecb )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.addedQuery;
            return new ReactiveDataAddJob() {
                compHandle  = sys.GetComponentTypeHandle<$$componentNameFull$$>( true ),
                entityHandle = sys.GetEntityTypeHandle(),
                ecb = ecb
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveDataAddJob : IJobEntityBatchWithIndex
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
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, ref compHandle );
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
                        PreviousValue = comp,
                        Added         = true,
                        Changed       = false,
                        Removed       = false,
                        _AddedCheck   = true
                    }
                };
                ecb.AddComponent( indexOfFirstEntityInQuery, entity, rComp );
            }
        }

        public static Unity.Jobs.JobHandle AddMissingReactiveEntityTagWithEntityManager( $$systemNameFull$$ sys, 
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            if( !instanceData.reactiveEntityTagQuery.IsEmpty ) {
                dependency.Complete();
                var entities = instanceData.reactiveEntityTagQuery.ToEntityArray( Allocator.Temp );
                foreach ( var e in entities ) {
                    sys.EntityManager.AddComponent<ReactiveEntityTag>( e );
                }
                entities.Dispose();
            }
            return dependency;
        }

        public static Unity.Jobs.JobHandle AddMissingReactiveEntityTag( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency, EntityCommandBuffer.ParallelWriter ecb )
        {
            return GetReactiveEntityTagAddJob( sys, out var query, ecb ).ScheduleParallel( query, dependency );
        }

        private static ReactiveEntityTagAddJob GetReactiveEntityTagAddJob( $$systemNameFull$$ sys,
            out EntityQuery query, EntityCommandBuffer.ParallelWriter ecb )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.reactiveEntityTagQuery;
            return new ReactiveEntityTagAddJob() {
                entityHandle = sys.GetEntityTypeHandle(),
                ecb = ecb
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveEntityTagAddJob : IJobEntityBatchWithIndex
        {
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery )
            {
                var entities      = batchInChunk.GetNativeArray( entityHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        entities[i],
                        ecb,
                        indexOfFirstEntityInQuery );
                }
            }

            private static void Execute( Entity entity, EntityCommandBuffer.ParallelWriter ecb, int indexOfFirstEntityInQuery )
            {
                ecb.AddComponent( indexOfFirstEntityInQuery, entity, new ReactiveEntityTag() );
            }
        }

        public static Unity.Jobs.JobHandle CheckForRemovedWithEntityManager( $$systemNameFull$$ sys, 
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            if( !instanceData.removedQuery.IsEmpty ) {
                dependency.Complete();
                var entities = instanceData.removedQuery.ToEntityArray( Allocator.Temp );
                foreach ( var e in entities ) {
                    var rComp     = sys.EntityManager.GetComponentData<$$reactiveComponentNameFull$$>( e );
                    var rCompData = rComp.Value;

                    if ( rCompData.Removed && rCompData._AddedCheck == false ) {
                        rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                            PreviousValue = rCompData.PreviousValue,
                            Added         = false,
                            Changed       = false,
                            Removed       = false,
                            _AddedCheck   = false
                        };
                    } else if( rCompData._AddedCheck == true ) {
                        rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                            PreviousValue = rCompData.PreviousValue,
                            Added         = false,
                            Changed       = false,
                            Removed       = true,
                            _AddedCheck   = false
                        };
                    }

                    sys.EntityManager.SetComponentData( e, rComp );
                }
                entities.Dispose();
            }
            return dependency;
        }

        public static Unity.Jobs.JobHandle CheckForRemoved( $$systemNameFull$$ sys, Unity.Jobs.JobHandle dependency )
        {
            return GetCheckForRemovedJob( sys, out var query ).ScheduleParallel( query, dependency );
        }

        private static CheckForRemovedJob GetCheckForRemovedJob( $$systemNameFull$$ sys, out EntityQuery query )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.removedQuery;
            return new CheckForRemovedJob() {
                rCompHandle = sys.GetComponentTypeHandle<$$reactiveComponentNameFull$$>( false )
            };
        }

        [Unity.Burst.BurstCompile]
        private struct CheckForRemovedJob : IJobEntityBatch
        {
            public ComponentTypeHandle<$$reactiveComponentNameFull$$> rCompHandle;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex )
            {
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr( batchInChunk, ref rCompHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<$$reactiveComponentNameFull$$>( rCompArrayPtr, i )
                        );
                }
            }

            private static void Execute( ref $$reactiveComponentNameFull$$ rComp )
            {
                var rCompData = rComp.Value;
                if ( rCompData.Removed && rCompData._AddedCheck == false ) {
                    rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                        PreviousValue = rCompData.PreviousValue,
                        Added         = false,
                        Changed       = false,
                        Removed       = false,
                        _AddedCheck   = false
                    };
                } else if( rCompData._AddedCheck == true ) {
                    rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                        PreviousValue = rCompData.PreviousValue,
                        Added         = false,
                        Changed       = false,
                        Removed       = true,
                        _AddedCheck   = false
                    };
                }
            }
        }

        public static Unity.Jobs.JobHandle CheckForChangedOrAddedWithEntityManager( $$systemNameFull$$ sys, 
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            if( !instanceData.changedQuery.IsEmpty ) {
                dependency.Complete();
                var entities = instanceData.changedQuery.ToEntityArray( Allocator.Temp );
                foreach ( var e in entities ) {
                    var rComp = sys.EntityManager.GetComponentData<$$reactiveComponentNameFull$$>( e );
                    var comp  = sys.EntityManager.GetComponentData<$$componentNameFull$$>( e );

                    // Component added or enabled AND ReactiveData already exists => ADDED
                    if( rComp.Value._AddedCheck == false ) {
                        rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                            PreviousValue = comp,
                            Added         = true,
                            Changed       = false,
                            Removed       = false,
                            _AddedCheck   = true
                        };     
                    } else {
                        // Check for CHANGED
                        rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                            PreviousValue = comp,
                            Added         = false,
                            Changed       = BooleanSimplifier.Any( comp.$$variableNameToCompare$$ != rComp.Value.PreviousValue.$$variableNameToCompare$$ ),
                            Removed       = rComp.Value.Removed,
                            _AddedCheck   = rComp.Value._AddedCheck
                        };
                    }

                    sys.EntityManager.SetComponentData( e, rComp );
                }
                entities.Dispose();
            }
            return dependency;
        }

        public static Unity.Jobs.JobHandle CheckForChangedOrAdded( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        {
            return GetCheckForChangedOrAddedJob( sys, out var query ).ScheduleParallel( query, dependency );
        }

        private static CheckForChangedOrAddedJob GetCheckForChangedOrAddedJob( $$systemNameFull$$ sys,
            out EntityQuery query )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.changedQuery;
            return new CheckForChangedOrAddedJob() {
                compHandle  = sys.GetComponentTypeHandle<$$componentNameFull$$>( true ),
                rCompHandle = sys.GetComponentTypeHandle<$$reactiveComponentNameFull$$>( false )
            };
        }

        [Unity.Burst.BurstCompile]
        private struct CheckForChangedOrAddedJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<$$componentNameFull$$> compHandle;
            public ComponentTypeHandle<$$reactiveComponentNameFull$$> rCompHandle;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex )
            {
                var compsArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, ref compHandle );
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr( batchInChunk, ref rCompHandle );
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
                // Component added or enabled AND ReactiveData already exists => ADDED
                if( rComp.Value._AddedCheck == false ) {
                    rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                        PreviousValue = comp,
                        Added         = true,
                        Changed       = false,
                        Removed       = false,
                        _AddedCheck   = true
                    };                                        
                    return;
                }

                // Check for CHANGED
                rComp.Value = new ComponentReactiveData<$$componentNameFull$$>() {
                    PreviousValue = comp,
                    Added         = false,
                    Changed       = BooleanSimplifier.Any( comp.$$variableNameToCompare$$ != rComp.Value.PreviousValue.$$variableNameToCompare$$ ),
                    Removed       = rComp.Value.Removed,
                    _AddedCheck   = rComp.Value._AddedCheck
                };
            }
        }

        public static Unity.Jobs.JobHandle CleanupDestroyedReactiveEntitiesWithEntityManager( $$systemNameFull$$ sys, 
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            if( !instanceData.cleanupQuery.IsEmpty ) {
                dependency.Complete();
                var entities = instanceData.cleanupQuery.ToEntityArray( Allocator.Temp );
                foreach ( var e in entities ) {
                    var rComp = sys.EntityManager.GetComponentData<$$reactiveComponentNameFull$$>( e );
                    if( rComp.Value._AddedCheck == false )
                        sys.EntityManager.RemoveComponent<$$reactiveComponentNameFull$$>( e );
                }
                entities.Dispose();
            }
            return dependency;
        }

        public static Unity.Jobs.JobHandle CleanupDestroyedReactiveEntities( $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency, EntityCommandBuffer.ParallelWriter ecb )
        {
            return GetReactiveEntityCleanupJob( sys, out var query, ecb ).ScheduleParallel( query, dependency );
        }

        private static ReactiveEntityCleanupJob GetReactiveEntityCleanupJob( $$systemNameFull$$ sys,
            out EntityQuery query, EntityCommandBuffer.ParallelWriter ecb )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.cleanupQuery;
            return new ReactiveEntityCleanupJob() {
                entityHandle = sys.GetEntityTypeHandle(),
                rCompHandle = sys.GetComponentTypeHandle<$$reactiveComponentNameFull$$>( true ),
                ecb = ecb
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveEntityCleanupJob : IJobEntityBatchWithIndex
        {
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            [ReadOnly]
            public ComponentTypeHandle<$$reactiveComponentNameFull$$> rCompHandle;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery )
            {
                var entities      = batchInChunk.GetNativeArray( entityHandle );
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, ref rCompHandle );
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
                if( rComp.Value._AddedCheck == false )
                    ecb.RemoveComponent<$$reactiveComponentNameFull$$>( indexOfFirstEntityInQuery, entity );
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
        { $$placeForAddMissingReactiveData_WithoutEcb$$
            return dependency;
        }

        /// <summary>
        /// Updates reactive components in parallel jobs with a temporary EntityCommandBuffer.
        /// This method is NOT recommended as it makes a sync point.
        /// </summary>
        public static Unity.Jobs.JobHandle UpdateReactiveNowWithEcb( this $$systemNameFull$$ sys,
            Unity.Jobs.JobHandle dependency )
        { $$placeForUpdatesChanged$$ $$placeForAddMissingReactiveData_WithTempEcb$$ $$placeForUpdatesRemoved$$
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
            var defaultEcb = sys.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
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
            var ecbForAdded      = ecbSystem.CreateCommandBuffer();
            var ecbForMissingTag = ecbSystem.CreateCommandBuffer();
            var ecbForCleanup    = ecbSystem.CreateCommandBuffer();
            dependency = UpdateReactive( sys, dependency, ecbForAdded, ecbForMissingTag, ecbForCleanup );
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
            Unity.Jobs.JobHandle dependency, EntityCommandBuffer ecbForAdded, EntityCommandBuffer ecbForMissingTag, 
            EntityCommandBuffer ecbForCleanup )
        {
$$placeForUpdatesChanged$$
            var ecbWriterForAdded      = ecbForAdded.AsParallelWriter();
            var ecbWriterForMissingTag = ecbForMissingTag.AsParallelWriter();
            var ecbWriterForCleanup    = ecbForCleanup.AsParallelWriter();
$$placeForAddMissingReactiveData_WithExternalEcb$$
$$placeForUpdatesRemoved$$
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

        public static string GetTemplateForSystemAddMissingReactiveData_WithoutEcb()
        {
            return @"
            dependency = $$systemName$$_$$componentName$$_Reactive.CheckForChangedOrAddedWithEntityManager( sys, dependency );
            dependency = $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveDataWithEntityManager( sys, dependency );
            dependency = $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveEntityTagWithEntityManager( sys, dependency );
            dependency = $$systemName$$_$$componentName$$_Reactive.CleanupDestroyedReactiveEntitiesWithEntityManager( sys, dependency );
            dependency = $$systemName$$_$$componentName$$_Reactive.CheckForRemovedWithEntityManager( sys, dependency );
";
        }

        public static string GetTemplateForSystemAddMissingReactiveData_WithTempEcb()
        {
            return @"
            var $$systemName$$_$$componentName$$_ecbForAdded = new EntityCommandBuffer( Allocator.TempJob );
            var $$systemName$$_$$componentName$$_ecbWriterForAdded = 
                $$systemName$$_$$componentName$$_ecbForAdded.AsParallelWriter();
            var $$systemName$$_$$componentName$$_ecbForMissingTag = new EntityCommandBuffer( Allocator.TempJob );
            var $$systemName$$_$$componentName$$_ecbWriterForMissingTag = 
                $$systemName$$_$$componentName$$_ecbForMissingTag.AsParallelWriter();
            var $$systemName$$_$$componentName$$_ecbForCleanup = new EntityCommandBuffer( Allocator.TempJob );
            var $$systemName$$_$$componentName$$_ecbWriterForCleanup = 
                $$systemName$$_$$componentName$$_ecbForCleanup.AsParallelWriter();

            dependency = $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveData( sys, dependency, $$systemName$$_$$componentName$$_ecbWriterForAdded );
            dependency = $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveEntityTag( sys, dependency, $$systemName$$_$$componentName$$_ecbWriterForMissingTag );
            dependency = $$systemName$$_$$componentName$$_Reactive.CleanupDestroyedReactiveEntities( sys, dependency, $$systemName$$_$$componentName$$_ecbWriterForCleanup );
            dependency.Complete();

            $$systemName$$_$$componentName$$_ecbForAdded.Playback( sys.EntityManager );
            $$systemName$$_$$componentName$$_ecbForAdded.Dispose();
            $$systemName$$_$$componentName$$_ecbForMissingTag.Playback( sys.EntityManager );
            $$systemName$$_$$componentName$$_ecbForMissingTag.Dispose();
            $$systemName$$_$$componentName$$_ecbForCleanup.Playback( sys.EntityManager );
            $$systemName$$_$$componentName$$_ecbForCleanup.Dispose();";
        }

        public static string GetTemplateForSystemAddMissingReactiveData_WithExternalEcb()
        {
            return @"
            dependency = $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveData( sys, dependency, ecbWriterForAdded );
            dependency = $$systemName$$_$$componentName$$_Reactive.AddMissingReactiveEntityTag( sys, dependency, ecbWriterForMissingTag );
            dependency = $$systemName$$_$$componentName$$_Reactive.CleanupDestroyedReactiveEntities( sys, dependency, ecbWriterForCleanup );";
        }

        public static string GetTemplateForChangedOrAddedCheck()
        {
            return
                "            dependency = $$systemName$$_$$componentName$$_Reactive.CheckForChangedOrAdded( sys, dependency );";
        }

        public static string GetTemplateForRemovedCheck()
        {
            return
                "            dependency = $$systemName$$_$$componentName$$_Reactive.CheckForRemoved( sys, dependency );";
        }

        public static string GetTemplateForReactiveComponent()
        {
            return @"        public struct $$componentName$$Reactive : ICleanupComponentData
        {
            public ComponentReactiveData<$$componentNameFull$$> Value;
        }";
        }
    }
}