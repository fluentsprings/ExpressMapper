using System.Collections.Generic;

namespace ExpressMapper.Ioc
{
    public interface IContainer
    {
        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
    }
}
