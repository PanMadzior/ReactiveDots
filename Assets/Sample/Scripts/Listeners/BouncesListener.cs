using ReactiveDots;
using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    public class BouncesListener : MonoBehaviour, IAnyBouncesListener
    {
        private void Awake()
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventSystem>().Events.AddAnyBouncesListener( this );
        }

        public void OnAnyBounces( Entity entity, Bounces bounces, World world )
        {
            Debug.Log(
                $"New bounce #{bounces.Value} by entity with id={world.EntityManager.GetComponentData<Id>( entity ).Value}" );
        }
    }
}