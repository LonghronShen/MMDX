using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    internal static class ContentWriterExtensions
    {
        internal static Dictionary<Type, MethodInfo> WriteMethods = new Dictionary<Type, MethodInfo>();

        static ContentWriterExtensions()
        {
            WriteMethods = typeof(ContentWriter).GetMethods()
                .Where(x => x.Name == "Write")
                .Select(x => new { Parameters = x.GetParameters().Select(z => z.ParameterType).ToArray(), MethodInfo = x })
                .Where(x => x.Parameters.Length == 1)
                .ToDictionary(x => x.Parameters[0], x => x.MethodInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteAny<T>(this ContentWriter self, T value)
        {
            if (typeof(T).IsValueType)
            {
                if (WriteMethods.TryGetValue(typeof(T), out var method))
                {
                    method.Invoke(self, new object[] { value });
                }
                else
                {
                    throw new NotSupportedException($"Type `{typeof(T).Name}` is not supported.");
                }
            }
            else
            {
                self.WriteObject(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDictionary<TKey, TValue>(this ContentWriter self, Dictionary<TKey, TValue> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            self.Write(value.Count);
            foreach (var pair in value)
            {
                self.WriteAny(pair.Key);
                self.WriteAny(pair.Value);
            }
        }
    }
}
