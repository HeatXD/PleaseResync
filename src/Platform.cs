using System;

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
    }
}
