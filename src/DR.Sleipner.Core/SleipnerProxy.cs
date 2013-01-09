using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.Core.ProxyGenerators;

namespace DR.Sleipner.Core
{
    public class SleipnerProxy<T> where T : class
    {
        private readonly T _realInstance;
        private readonly IInterceptHandler<T> _interceptHandler;
        public T Object;

        public SleipnerProxy(T realInstance, IInterceptHandler<T> interceptHandler)
        {
            _realInstance = realInstance;
            _interceptHandler = interceptHandler;

            Object = ProxyGeneratorCache.GetProxy(realInstance, interceptHandler);
        }
    }
}