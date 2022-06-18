using Unity.Mathematics;

namespace ReactiveDots
{
    public static class BooleanSimplifier
    {
        public static bool All( bool result ) => result;
        public static bool All( bool2 result ) => result.x && result.y;
        public static bool All( bool3 result ) => result.x && result.y && result.z;
        public static bool All( bool4 result ) => result.x && result.y && result.z && result.w;

        public static bool Any( bool result ) => result;
        public static bool Any( bool2 result ) => result.x || result.y;
        public static bool Any( bool3 result ) => result.x || result.y || result.z;
        public static bool Any( bool4 result ) => result.x || result.y || result.z || result.w;
    }
}