using System;
using System.Reflection;

namespace ExpressMapper
{
    public static class TypeExtensions
    {
        public static TypeInfo GetInfo(this Type type)
        {
            return type.GetTypeInfo();
        }
    }
}