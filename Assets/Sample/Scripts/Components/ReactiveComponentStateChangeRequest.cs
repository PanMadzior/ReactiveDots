using Unity.Entities;

namespace ReactiveDotsSample
{
    public struct ReactiveComponentStateChangeRequest : IComponentData
    {
        public bool newState;
    }
}