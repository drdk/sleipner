using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DR.Sleipner.CacheConfiguration;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.CacheProxy.Syncronizer;
using DR.Sleipner.Model;

namespace DR.Sleipner
{
    public class SleipnerProxy<T>    where T : class
    {
        private readonly T _realInstance;
        public T Object { get; private set; }

        private readonly IProxyHandler<T> _proxyHandler;
        private readonly ICachePolicyProvider<T> _cachePolicyProvider;

        public SleipnerProxy(T realInstance, ICacheProvider<T> cacheProvider)
        {
            if (!typeof (T).IsInterface)
            {
                throw new ArgumentException("T must be an interface", "realInstance");
            }

            var proxyType = CacheProxyGenerator.GetProxyType<T>();

            _realInstance = realInstance;
            _cachePolicyProvider = new CachePolicyProvider<T>();
            _proxyHandler = new ThrottledProxyHandler<T>(_realInstance, _cachePolicyProvider, cacheProvider);
            
            Object = (T)Activator.CreateInstance(proxyType, _realInstance, _proxyHandler);
        }

        public void ConfigureCaching(Action<ICachePolicyProvider<T>> expression)
        {
            expression(_cachePolicyProvider);
        }
    }
}