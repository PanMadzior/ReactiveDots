using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ReactiveDotsSample
{
    public class BallAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float2 minMaxSpeed;

        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            dstManager.AddComponent<Ball>( entity );
            dstManager.AddComponentData( entity, Id.CreateNew() );
            dstManager.AddComponentData( entity, new MoveDirection() {
                Value = math.mul( quaternion.RotateY( Random.Range( 0, 360 ) ), new float3( 0f, 0f, 1f ) )
            } );
            dstManager.AddComponentData( entity, new Bounces() { Value = 0 } );
            dstManager.AddComponentData( entity, new Speed() { Value = Random.Range( minMaxSpeed.x, minMaxSpeed.y ) } );
        }
    }
}