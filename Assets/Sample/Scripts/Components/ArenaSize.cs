using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ReactiveDotsSample
{
    public struct ArenaSize : IComponentData
    {
        public float2 Value;
    }

    public class ArenaSizeAuthoring : MonoBehaviour
    {
        public float Value;
    }

    public class MyComponentAuthoringBaker : Baker<ArenaSizeAuthoring>
    {
        public override void Bake( ArenaSizeAuthoring authoring )
        {
            AddComponent( new ArenaSize {
                Value = authoring.Value
            } );
        }
    }
}