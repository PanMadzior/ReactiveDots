using ReactiveDots;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [ReactiveEvent( EventType.All )]
    public struct Bounces : IComponentData
    {
        public int Value;
    }
}