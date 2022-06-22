using Unity.Entities;

namespace ReactiveDots
{
    public struct ComponentReactiveData<T> where T : IComponentData
    {
        public T    PreviousValue;
        public bool Changed;
        public bool Added;
        public bool Removed;

        public bool AddedOrChanged => Added || Changed;
        public bool AddedOrRemoved => Added || Removed;
        public bool ChangedOrRemoved => Changed || Removed;
    }
}