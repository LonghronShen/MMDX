using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Content
{
    internal static class ContentReaderExtensions
    {
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
