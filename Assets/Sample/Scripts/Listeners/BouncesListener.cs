using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    public class BouncesListener : MonoBehaviour, IAnyBouncesAddedListener, IAnyBouncesChangedListener,
        IAnyBouncesRemovedListener
    {
        private void Awake()
        {
            var entityManager  = World.DefaultGameObjectInjectionWorld.EntityManager;
            var listenerEntity = entityManager.CreateEntity();
            entityManager.AddComponentData( listenerEntity, new AnyBouncesAddedListener() { Value   = this } );
            entityManager.AddComponentData( listenerEntity, new AnyBouncesRemovedListener() { Value = this } );
            entityManager.AddComponentData( listenerEntity, new AnyBouncesChangedListener() { Value = this } );
        }

        public void OnAnyBouncesAdded( Entity entity, Bounces bounces, World world )
        {
            Debug.Log(
                $"New bounce #{bounces.Value} for entity with id={entity.Index}" );
        }

        public void OnAnyBouncesChanged( Entity entity, Bounces bounces, World world )
        {
            Debug.Log(
                $"Updated bounce #{bounces.Value} for entity with id={entity.Index}" );
        }

        public void OnAnyBouncesRemoved( Entity entity, World world )
        {
            Debug.Log(
                $"Bounce removed from entity with id={entity.Index}" );
        }
    }
}