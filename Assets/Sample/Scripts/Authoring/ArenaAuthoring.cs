using Unity.Entities;
using UnityEngine;

namespace ReactiveDotsSample
{
    public class ArenaAuthoring : MonoBehaviour
    {
        public float      size;
        public GameObject ballPrefab;

        public class Baker : Baker<ArenaAuthoring>
        {
            public override void Bake( ArenaAuthoring authoring )
            {
                AddComponent( new Arena {
                    Size       = authoring.size,
                    BallPrefab = GetEntity( authoring.ballPrefab )
                } );
            }
        }
    }
}