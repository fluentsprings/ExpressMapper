using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExpressMapper
{
    internal static class CollectionExtentions
    {
        internal static Collection<T> ToCollection<T>(this IList<T> source)
        {
            var collection = new Collection<T>(source);
            return collection;
        }
    }
}
