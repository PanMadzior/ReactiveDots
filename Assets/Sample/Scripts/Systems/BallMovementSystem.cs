using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ReactiveDotsSample
{
    public partial class BallMovementSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Arena>();
        }

        protected override void OnUpdate()
        {
            var arenaSize   = GetSingleton<Arena>().Size;
            var arenaBounds = new float4( -arenaSize.x / 2f, -arenaSize.y / 2f, arenaSize.x / 2f, arenaSize.y / 2f );
            var dt          = SystemAPI.Time.DeltaTime;

            Entities.ForEach( ( ref MoveDirection direction, ref LocalToWorldTransform transform, in Speed speed ) =>
            {
                var pos       = transform.Value.Position;
                var moveDelta = direction.Value * speed.Value * dt;
                var newPos    = pos + moveDelta;
                if ( IsInBounds( newPos, arenaBounds ) )
                    transform.Value.Position = newPos;
                else
                    direction.Value = GetNewDirection( direction.Value, newPos, arenaBounds );
            } ).ScheduleParallel();
        }

        private static float3 GetNewDirection( float3 directionValue, float3 pos, float4 bounds )
        {
            if ( pos.x < bounds.x )
                directionValue.x *= -1;
            if ( pos.z < bounds.y )
                directionValue.z *= -1;
            if ( pos.x > bounds.z )
                directionValue.x *= -1;
            if ( pos.z > bounds.w )
                directionValue.z *= -1;
            return directionValue;
        }

        private static bool IsInBounds( float3 pos, float4 bounds )
        {
            if ( pos.x < bounds.x )
                return false;
            if ( pos.z < bounds.y )
                return false;
            if ( pos.x > bounds.z )
                return false;
            if ( pos.z > bounds.w )
                return false;
            return true;
        }
    }
}