using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    public class SpeedListener : MonoBehaviour, IAnySpeedAddedListener, IAnySpeedChangedListener,
        IAnySpeedRemovedListener
    {
        private void Awake()
        {
            var entityManager  = World.DefaultGameObjectInjectionWorld.EntityManager;
            var listenerEntity = entityManager.CreateEntity();
            entityManager.AddComponentData( listenerEntity, new AnySpeedAddedListener() { Value   = this } );
            entityManager.AddComponentData( listenerEntity, new AnySpeedRemovedListener() { Value = this } );
            entityManager.AddComponentData( listenerEntity, new AnySpeedChangedListener() { Value = this } );
        }

        public void OnAnySpeedAdded( Entity entity, Speed component, World world )
        {
            Debug.Log(
                $"New speed={component.Value} for entity with id={entity.Index}" );
        }

        public void OnAnySpeedChanged( Entity entity, Speed component, World world )
        {
            Debug.Log(
                $"Updated speed={component.Value} for entity with id={entity.Index}" );
        }

        public void OnAnySpeedRemoved( Entity entity, World world )
        {
            Debug.Log(
                $"Speed removed from entity with id={entity.Index}" );
        }
    }
}