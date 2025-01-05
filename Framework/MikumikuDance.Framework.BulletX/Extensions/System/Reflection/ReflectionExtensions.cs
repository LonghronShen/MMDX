using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    internal static class ReflectionExtensions
    {

#if NET40
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
#endif

    }
}
