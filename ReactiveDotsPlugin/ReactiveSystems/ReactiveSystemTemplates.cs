namespace ReactiveDotsPlugin
{
    public static class ReactiveSystemTemplates
    {
        public static string GetReactiveSystemTemplate()
        {
            return @"// Auto Generated Code
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using ReactiveDots;

namespace NAMESPACENAME
{
    public static class SYSNAME_Reactive
    {
        private struct InstanceData
        {
            public EntityQuery addedQuery;
            public EntityQuery removedQuery;
            public EntityQuery changedQuery;
        }

        private static Dictionary<SYSNAME, InstanceData> Instances =
            new Dictionary<SYSNAME, InstanceData>();

        private static InstanceData GetOrCreateInstanceData( SYSNAME sys )
        {
            if ( !Instances.ContainsKey( sys ) )
                Instances.Add( sys, CreateInstanceData( sys ) );
            return Instances[sys];
        }

        private static InstanceData CreateInstanceData( SYSNAME sys )
        {
            var data = new InstanceData();
            data.addedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<COMPONENTFULL>(),
                ComponentType.Exclude<RCOMPONENTFULL>()
            );
            data.removedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.Exclude<COMPONENTFULL>(),
                ComponentType.ReadWrite<RCOMPONENTFULL>()
            );
            data.changedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<COMPONENTFULL>(),
                ComponentType.ReadWrite<RCOMPONENTFULL>()
            );
            return data;
        }

        public static Unity.Jobs.JobHandle UpdateReactive( this SYSNAME sys,
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            UpdateAdded( sys, instanceData );
            UpdateRemoved( sys, instanceData );
            return GetReactiveUpdateJob( sys, out var query ).ScheduleParallel( query, dependency );
        }

        private static void UpdateAdded( SYSNAME sys, InstanceData instanceData )
        {
            var addedEntities = instanceData.addedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in addedEntities ) {
                sys.EntityManager.AddComponentData( e, new RCOMPONENTFULL() {
                    Value = new ComponentReactiveData<COMPONENTFULL>() {
                        PreviousValue        = sys.EntityManager.GetComponentData<COMPONENTFULL>( e ),
                        Added                = true,
                        Changed              = true,
                        Removed              = false,
                        _FirstCheckCompleted = false
                    }
                } );
            }

            addedEntities.Dispose();
        }

        private static void UpdateRemoved( SYSNAME sys, InstanceData instanceData )
        {
            var removedEntities = instanceData.removedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in removedEntities ) {
                var rComp = sys.EntityManager.GetComponentData<RCOMPONENTFULL>( e );
                var rCompData = rComp.Value;
                if ( rCompData.Removed )
                    sys.EntityManager.RemoveComponent<RCOMPONENTFULL>( e );
                else {
                    rCompData.Added   = false;
                    rCompData.Changed = false;
                    rCompData.Removed = true;
                    rComp.Value       = rCompData;
                }
            }

            removedEntities.Dispose();
        }

        private static ReactiveUpdateJob GetReactiveUpdateJob( this SYSNAME sys,
            out EntityQuery query )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.changedQuery;
            return new ReactiveUpdateJob() {
                compHandle  = sys.GetComponentTypeHandle<COMPONENTFULL>( true ),
                rCompHandle = sys.GetComponentTypeHandle<RCOMPONENTFULL>( false )
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveUpdateJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<COMPONENTFULL> compHandle;
            public ComponentTypeHandle<RCOMPONENTFULL> rCompHandle;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex )
            {
                var compsArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, compHandle );
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr( batchInChunk, rCompHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<COMPONENTFULL>( compsArrayPtr, i ),
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<RCOMPONENTFULL>(
                                rCompArrayPtr, i ) 
                        );
                }
            }

            private static void Execute( ref COMPONENTFULL comp, ref RCOMPONENTFULL rComp )
            {
                rComp.Value = new ComponentReactiveData<COMPONENTFULL>() {
                    PreviousValue        = comp,
                    Added                = !rComp.Value._FirstCheckCompleted,
                    Changed              = rComp.Value.Added || BooleanSimplifier.Any( comp.VARIABLENAMETOCOMPARE != rComp.Value.PreviousValue.VARIABLENAMETOCOMPARE ),
                    Removed              = false,
                    _FirstCheckCompleted = true
                };
            }
        }
    }
}
";
        }
    }
}