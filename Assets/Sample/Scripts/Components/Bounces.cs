using ReactiveDots;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [ReactiveEvent( EventType.All )]
    public struct Bounces : IComponentData, IEnableableComponent
    {
        public int Value;
    }
}