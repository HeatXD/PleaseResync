using System;

namespace PleaseResync
{
    public static class Platform
    {
        public static uint GetCurrentTimeMS()
        {
            return (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
