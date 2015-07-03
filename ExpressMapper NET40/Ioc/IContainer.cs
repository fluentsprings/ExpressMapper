using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExpressMapper.Ioc
{
    public interface IContainer
    {
        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
    }
}
