using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ExpressMapper.Extensions
{
    public static class ExpressmapperExtensions
    {
        public static TN Map<T, TN>(this T source)
        {
            return Mapper.Map<T, TN>(source);
        }

        public static TN Map<T, TN>(this T source, TN destination)
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

        public static IQueryable<TN> Project<T, TN>(this IQueryable<T> source)
        {
            return Mapper.Project<T, TN>(source);
        }
    }
}
