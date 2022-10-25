using Unity.Entities;

namespace ReactiveDots
{
    public struct ReactiveEntityTag : IComponentData { }

    public struct ReactiveEntityCleanupTag : ICleanupComponentData { }
}