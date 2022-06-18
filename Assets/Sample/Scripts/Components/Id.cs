using Unity.Entities;

namespace ReactiveDotsSample
{
    public struct Id : IComponentData
    {
        public int Value;

        private static int s_lastId;

        public static Id CreateNew()
        {
            return new Id() {
                Value = ++s_lastId
            };
        }
    }
}