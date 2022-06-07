using Unity.Entities;

namespace ReactiveDotsSample
{
    public struct Id : IComponentData
    {
        public static int LastId;
        public        int Value;

        public static Id CreateNew()
        {
            return new Id() {
                Value = ++LastId
            };
        }
    }
}