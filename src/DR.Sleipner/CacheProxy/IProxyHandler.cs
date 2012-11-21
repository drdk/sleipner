using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.CacheProxy
{
    public interface IProxyHandler<TType> where TType : class
    {
        TResult HandleRequest<TResult>(string methodName, object[] parameters);
    }
}
