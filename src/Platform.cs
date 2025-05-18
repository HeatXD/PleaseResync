using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PleaseResync
{
    internal static class Platform
    {
        private readonly static Random RandomNumberGenerator = new();
        public enum DebugType{ Log, Warning, Error};

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

        //https://en.wikipedia.org/wiki/Fletcher%27s_checksum
        public static uint FletcherChecksum(ushort[] data, int len)
        {
            uint sum1 = 0xffff, sum2 = 0xffff;
            int index  = 0;

            while (len > 0)
            {
                int tlen = len > 360 ? 360 : len;
                len -= tlen;
                do
                {
                    sum1 += data[index++];
                    sum2 += sum1;
                } while (--tlen > 0);

                sum1 = (sum1 & 0xffff) + (sum1 >> 16);
                sum2 = (sum2 & 0xffff) + (sum2 >> 16);
            }

            /* Second reduction step to reduce sums to 16 bits */
            sum1 = (sum1 & 0xffff) + (sum1 >> 16);
            sum2 = (sum2 & 0xffff) + (sum2 >> 16);
            return sum2 << 16 | sum1;
        }
        public static void Log(string message = "", DebugType type = DebugType.Log)
        {
#if DEBUG
            switch (type)
            {
                case DebugType.Log:
                    Console.WriteLine($"LOG: {message}");
                    break;
                case DebugType.Warning:
                    Console.WriteLine($"WARNING: {message}");
                    break;
                case DebugType.Error:
                    Console.WriteLine($"ERROR: {message}");
                    break;
            }
#endif
        }

        public static List<byte> RLEEncode(List<byte> toEncode)
        {
            if (toEncode == null || toEncode.Count == 0)
                return [];

            var bytes = new List<byte>();
            byte count = 1;
            byte current = toEncode[0];

            for (int i = 1; i < toEncode.Count; i++)
            {
                if (toEncode[i] == current && count < byte.MaxValue)
                {
                    count++;
                }
                else
                {
                    bytes.Add(count);
                    bytes.Add(current);
                    current = toEncode[i];
                    count = 1;
                }
            }

            // add the last run
            bytes.Add(count);
            bytes.Add(current);

            return bytes;
        }

        public static List<byte> RLEDecode(List<byte> data)
        {
            if (data == null || data.Count % 2 != 0)
                throw new ArgumentException("Invalid RLE data. Must be in (count, value) pairs.");

            var result = new List<byte>();

            for (int i = 0; i < data.Count; i += 2)
            {
                byte count = data[i];
                byte value = data[i + 1];

                for (int j = 0; j < count; j++)
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }
}
