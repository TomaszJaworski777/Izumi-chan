using System.Runtime.CompilerServices;

namespace Izumi.Misc
{
    internal static class Utils
    {
        private static Random _random = new(7777);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get32Random() => _random.Next();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Get64Random()
        {
            ulong u1, u2, u3, u4;
            u1 = (ulong)_random.Next() & 0xFFFF; u2 = (ulong)_random.Next() & 0xFFFF;
            u3 = (ulong)_random.Next() & 0xFFFF; u4 = (ulong)_random.Next() & 0xFFFF;
            return u1 | u2 << 16 | u3 << 32 | u4 << 48;
        }
    }
}
