using ReactiveDots;
using Unity.Entities;
using Unity.Mathematics;

namespace ReactiveDotsSample
{
    [UpdateBefore( typeof(BallMovementSystem) )]
    [ReactiveSystem( typeof(MoveDirection), typeof(MoveDirectionReactive) )]
    public partial class BallInitSystem : SystemBase
    {
        public struct MoveDirectionReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<MoveDirection> Value;
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactive( Dependency );
            Entities.WithNone<MoveDirectionReactive>().ForEach(
                ( Entity e, ref MoveDirection moveDir, ref Speed speed ) =>
                {
                    var random      = Random.CreateFromIndex( (uint)e.Index );
                    var randomRot   = random.NextFloat( 0, 360 );
                    var randomSpeed = random.NextFloat( 1f, 5f );
                    moveDir.Value = math.mul( quaternion.RotateY( randomRot ), new float3( 0f, 0f, 1f ) );
                    speed.Value   = randomSpeed;
                } ).ScheduleParallel();
        }
    }
}