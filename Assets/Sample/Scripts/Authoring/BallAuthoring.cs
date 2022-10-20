using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    public class BallAuthoring : MonoBehaviour
    {
        public class Baker : Baker<BallAuthoring>
        {
            public override void Bake( BallAuthoring authoring )
            {
                AddComponent( new ComponentTypeSet(
                    typeof(Ball),
                    typeof(MoveDirection),
                    typeof(Bounces),
                    typeof(Speed) )
                );
            }
        }
    }
}