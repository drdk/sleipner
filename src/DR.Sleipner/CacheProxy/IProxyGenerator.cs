using System;

namespace DR.Sleipner.CacheProxy
{
    public interface IProxyGenerator
    {
        Type CreateProxy<T>() where T : class;
    }
}