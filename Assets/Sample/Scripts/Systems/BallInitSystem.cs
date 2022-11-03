using ReactiveDots;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ReactiveDotsSample
{
    [UpdateBefore( typeof(BallMovementSystem) )]
    [ReactiveSystem( typeof(MoveDirection), typeof(MoveDirectionReactive) )]
    public partial class BallInitSystem : SystemBase
    {
        public struct MoveDirectionReactive : ICleanupComponentData
        {
            public ComponentReactiveData<MoveDirection> Value;
        }

        protected override void OnUpdate()
        {
            Dependency = Entities.WithNone<MoveDirectionReactive>().ForEach(
                ( Entity e, ref MoveDirection moveDir, ref Speed speed ) =>
                {
                    var random      = Unity.Mathematics.Random.CreateFromIndex( (uint)e.Index );
                    var randomRot   = random.NextFloat( 0, 360 );
                    var randomSpeed = random.NextFloat( 1f, 5f );
                    moveDir.Value = math.mul( quaternion.RotateY( randomRot ), new float3( 0f, 0f, 1f ) );
                    speed.Value   = randomSpeed;
                } ).ScheduleParallel( Dependency );
            Dependency = this.UpdateReactive( Dependency );
        }
    }
}