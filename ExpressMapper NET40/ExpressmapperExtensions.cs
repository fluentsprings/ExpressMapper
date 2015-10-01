using System;
using System.Runtime.CompilerServices;

namespace ExpressMapper.Extensions
{
    public static class ExpressmapperExtensions
    {
        public static TN Map<T, TN>(this T source, TN destination = default(TN))
        {
            return Mapper.Map(source, destination);
        }

        public static object Map(this object source, object destination = null)
        {
            return Mapper.Map(source, destination);
        }

        public static object Map(this object source, Type srcType, Type destType)
        {
            return Mapper.Map(source, srcType, destType);
        }
    }
}
