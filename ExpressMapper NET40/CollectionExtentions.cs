using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExpressMapper
{
    public static class CollectionExtentions
    {
        public static Collection<T> ToCollection<T>(this IList<T> source)
        {
            var collection = new Collection<T>(source);
            return collection;
        }
    }
}
