using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Content
{
    internal static class ContentReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Read7BitEncodedInt(this ContentReader self)
        {
            int num = 0;
            int num2 = 0;
            byte b;
            do
            {
                if (num2 == 35)
                {
                    throw new FormatException("Format_Bad7BitInt32");
                }

                b = self.ReadByte();
                num |= (b & 0x7F) << num2;
                num2 += 7;
            }
            while ((b & 0x80u) != 0);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this ContentReader self)
        {
            var dict = new Dictionary<TKey, TValue>();
            var count = self.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var key = self.ReadObject<TKey>();
                var value = self.ReadObject<TValue>();
                dict.Add(key, value);
            }
            return dict;
        }
    }
}
