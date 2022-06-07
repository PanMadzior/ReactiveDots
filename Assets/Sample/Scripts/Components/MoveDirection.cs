using Unity.Entities;
using Unity.Mathematics;

namespace ReactiveDotsSample
{
    public struct MoveDirection : IComponentData
    {
        public float3 Value;
    }
}