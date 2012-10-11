using System;

namespace DR.Sleipner.CacheProxy
{
    public interface IProxyGenerator
    {
        Type CreateProxy<T, TImpl>() where T : class where TImpl : class, T;
    }
}