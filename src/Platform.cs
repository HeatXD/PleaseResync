using System;
using System.Runtime.CompilerServices;

namespace PleaseResync
{
    internal static class Platform
    {
        private readonly static Random RandomNumberGenerator = new Random();

        public static uint GetCurrentTimeMS()
        {
            return (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static ushort GetRandomUnsignedShort()
        {
            return (ushort)RandomNumberGenerator.Next();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] CloneByteArray(byte[] array)
        {
            var newArray = new byte[array.Length];
            Array.Copy(array, 0, newArray, 0, array.Length);
            return newArray;
        }
    }
}
