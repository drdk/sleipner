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
        private readonly ILookupHandler<T> _lookupHandler;
        public T Object;

        public SleipnerProxy(T realInstance, ILookupHandler<T> lookupHandler)
        {
            _realInstance = realInstance;
            _lookupHandler = lookupHandler;

            Object = ProxyGeneratorCache.GetProxy(realInstance, lookupHandler);
        }
    }
}