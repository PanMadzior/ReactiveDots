using System.Collections.Generic;
using ReactiveDotsSample;
using Unity.Collections;
using Unity.Entities;

namespace ReactiveDots
{
    public interface IAnyBouncesListener
    {
        void OnAnyBounces( Entity entity, Bounces bounces, World world );
    }

    public partial class ReactiveEvents
    {
        private List<IAnyBouncesListener> _listeners = new List<IAnyBouncesListener>();

        public List<IAnyBouncesListener> GetAnyBouncesListeners()
        {
            return _listeners;
        }

        public void AddAnyBouncesListener( IAnyBouncesListener listener )
        {
            _listeners.Add( listener );
        }

        public void RemoveAnyBouncesListener( IAnyBouncesListener listener )
        {
            if ( _listeners.Contains( listener ) )
                _listeners.Remove( listener );
        }
    }
}





namespace ReactiveDots
{
    public static class EventSystem_Reactive
    {
        private struct InstanceData
        {
            public EntityQuery addedQuery;
            public EntityQuery removedQuery;
            public EntityQuery changedQuery;
        }

        private static Dictionary<EventSystem, InstanceData> Instances =
            new Dictionary<EventSystem, InstanceData>();

        private static InstanceData GetOrCreateInstanceData( EventSystem sys )
        {
            if ( !Instances.ContainsKey( sys ) )
                Instances.Add( sys, CreateInstanceData( sys ) );
            return Instances[sys];
        }

        private static InstanceData CreateInstanceData( EventSystem sys )
        {
            var data = new InstanceData();
            data.addedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<ReactiveDotsSample.Bounces>(),
                ComponentType.Exclude<ReactiveDots.EventSystem.BouncesReactive>()
            );
            data.removedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.Exclude<ReactiveDotsSample.Bounces>(),
                ComponentType.ReadWrite<ReactiveDots.EventSystem.BouncesReactive>()
            );
            data.changedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<ReactiveDotsSample.Bounces>(),
                ComponentType.ReadWrite<ReactiveDots.EventSystem.BouncesReactive>()
            );
            return data;
        }

        public static Unity.Jobs.JobHandle UpdateReactiveEvents( this EventSystem sys,
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            UpdateAdded( sys, instanceData );
            UpdateRemoved( sys, instanceData );
            return GetReactiveUpdateJob( sys, out var query ).ScheduleParallel( query, dependency );
        }

        private static void UpdateAdded( EventSystem sys, InstanceData instanceData )
        {
            var addedEntities = instanceData.addedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in addedEntities ) {
                sys.EntityManager.AddComponentData( e, new ReactiveDots.EventSystem.BouncesReactive() {
                    Value = new ComponentReactiveData<ReactiveDotsSample.Bounces>() {
                        PreviousValue        = sys.EntityManager.GetComponentData<ReactiveDotsSample.Bounces>( e ),
                        Added                = true,
                        Changed              = true,
                        Removed              = false,
                        _FirstCheckCompleted = false
                    }
                } );
            }

            addedEntities.Dispose();
        }

        private static void UpdateRemoved( EventSystem sys, InstanceData instanceData )
        {
            var removedEntities = instanceData.removedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in removedEntities ) {
                var rComp = sys.EntityManager.GetComponentData<ReactiveDots.EventSystem.BouncesReactive>( e );
                var rCompData = rComp.Value;
                if ( rCompData.Removed )
                    sys.EntityManager.RemoveComponent<ReactiveDots.EventSystem.BouncesReactive>( e );
                else {
                    rCompData.Added   = false;
                    rCompData.Changed = false;
                    rCompData.Removed = true;
                    rComp.Value       = rCompData;
                }
            }

            removedEntities.Dispose();
        }

        private static ReactiveUpdateJob GetReactiveUpdateJob( this EventSystem sys,
            out EntityQuery query )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.changedQuery;
            return new ReactiveUpdateJob() {
                compHandle  = sys.GetComponentTypeHandle<ReactiveDotsSample.Bounces>( true ),
                rCompHandle = sys.GetComponentTypeHandle<ReactiveDots.EventSystem.BouncesReactive>( false )
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveUpdateJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<ReactiveDotsSample.Bounces> compHandle;
            public ComponentTypeHandle<ReactiveDots.EventSystem.BouncesReactive> rCompHandle;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex )
            {
                var compsArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, compHandle );
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr( batchInChunk, rCompHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<ReactiveDotsSample.Bounces>( compsArrayPtr, i ),
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<ReactiveDots.EventSystem.BouncesReactive>(
                                rCompArrayPtr, i ) 
                        );
                }
            }

            private static void Execute( ref ReactiveDotsSample.Bounces comp, ref ReactiveDots.EventSystem.BouncesReactive rComp )
            {
                rComp.Value = new ComponentReactiveData<ReactiveDotsSample.Bounces>() {
                    PreviousValue        = comp,
                    Added                = !rComp.Value._FirstCheckCompleted,
                    Changed              = rComp.Value.Added || BooleanSimplifier.Any( comp.Value != rComp.Value.PreviousValue.Value ),
                    Removed              = false,
                    _FirstCheckCompleted = true
                };
            }
        }
    }
}