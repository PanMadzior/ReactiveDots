using ReactiveDots;
using Unity.Entities;

namespace ReactiveDotsSample
{
    [ReactiveEvent( EventType.All, typeof(EventSystem) )]
    public struct Bounces : IComponentData
    {
        public int Value;
    }
}