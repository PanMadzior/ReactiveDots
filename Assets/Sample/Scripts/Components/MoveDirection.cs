using Unity.Entities;
using Unity.Mathematics;

namespace ReactiveDotsSample
{
    public struct MoveDirection : IComponentData, IEnableableComponent
    {
        public float3 Value;
        public int    Test;
    }
}