using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    public class BallAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            dstManager.AddComponents( entity, new ComponentTypeSet(
                typeof(Ball),
                typeof(MoveDirection),
                typeof(Bounces),
                typeof(Speed) )
            );
        }
    }
}