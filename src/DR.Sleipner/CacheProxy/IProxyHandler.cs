using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.CacheProxy
{
    public interface IProxyHandler<T> where T : class
    {
        TResult HandleRequest<TResult>(ProxyRequest<T, TResult> proxyRequest);
        TResult GetRealResult<TResult>(ProxyRequest<T, TResult> proxyRequest);
    }
}
