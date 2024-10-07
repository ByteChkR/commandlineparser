#if NET40
using System;

namespace CommandLine
{
    internal static class IntrospectionExtensions
    {
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
    }
}
#endif
