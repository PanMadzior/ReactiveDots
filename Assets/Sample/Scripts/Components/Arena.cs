using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ReactiveDotsSample
{
    public struct Arena : IComponentData
    {
        public float2 Size;
        public Entity BallPrefab;
    }
}