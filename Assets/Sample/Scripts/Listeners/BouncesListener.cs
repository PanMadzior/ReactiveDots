using ReactiveDots;
using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    public class BouncesListener : MonoBehaviour, EventSystem.IAnyBouncesAddedListener,
        EventSystem.IAnyBouncesChangedListener, EventSystem.IAnyBouncesRemovedListener
    {
        private void Awake()
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventSystem>().Events
                .AddAnyBouncesAddedListener( this );
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventSystem>().Events
                .AddAnyBouncesChangedListener( this );
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventSystem>().Events
                .AddAnyBouncesRemovedListener( this );
        }

        public void OnAnyBouncesAdded( Entity entity, Bounces bounces, World world )
        {
            Debug.Log(
                $"New bounce #{bounces.Value} by entity with id={world.EntityManager.GetComponentData<Id>( entity ).Value}" );
        }

        public void OnAnyBouncesChanged( Entity entity, Bounces bounces, World world )
        {
            Debug.Log(
                $"Updated bounce #{bounces.Value} by entity with id={world.EntityManager.GetComponentData<Id>( entity ).Value}" );
        }

        public void OnAnyBouncesRemoved( Entity entity, World world )
        {
            Debug.Log(
                $"Bounce removed in entity with id={world.EntityManager.GetComponentData<Id>( entity ).Value}" );
        }
    }
}