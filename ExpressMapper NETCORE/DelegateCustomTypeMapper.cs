using System;

namespace ExpressMapper
{
    internal class DelegateCustomTypeMapper<T, TN> : ICustomTypeMapper<T, TN>
    {
        private readonly Func<T, TN> _mapFunc;

        public DelegateCustomTypeMapper(Func<T, TN> mapFunc)
        {
            _mapFunc = mapFunc;
        }

        public TN Map(IMappingContext<T, TN> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return _mapFunc(context.Source);
        }
    }
}
