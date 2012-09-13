using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxyCore
{
    public interface IProxyGenerator
    {
        Type CreateProxy<T>() where T : class;
    }
}