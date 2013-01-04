using System;

namespace DR.Sleipner.Core
{
    public interface IProxyGenerator
    {
        Type CreateProxy<T>() where T : class;
    }
}