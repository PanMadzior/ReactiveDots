using Unity.Entities;
using Unity.Mathematics;

namespace ReactiveDotsSample
{
    [GenerateAuthoringComponent]
    public struct ArenaSize : IComponentData
    {
        public float2 Value;
    }
}