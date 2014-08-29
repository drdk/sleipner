using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.CacheProxy.Syncronizer;
using DR.Sleipner.Config;
using DR.Sleipner.Helpers;
using DR.Sleipner.Model;

namespace DR.Sleipner
{
    public class SleipnerProxy<T> where T : class
    {
        public T Object { get; private set; }
        public readonly ICachePolicyProvider<T> CachePolicyProvider;
        internal IList<IConfiguredMethod<T>> ConfiguredMethods = new List<IConfiguredMethod<T>>(); 

        public SleipnerProxy(T realInstance, ICacheProvider<T> cacheProvider)
        {
            if (!typeof (T).IsInterface)
            {
                throw new ArgumentException("T must be an interface", "realInstance");
            }

            var proxyType = CacheProxyGenerator.GetProxyType<T>();

            T realInstance1 = realInstance;
            CachePolicyProvider = new BasicConfigurationProvider<T>();
            IProxyHandler<T> proxyHandler = new ThrottledProxyHandler<T>(realInstance1, CachePolicyProvider, cacheProvider);
            
            Object = (T)Activator.CreateInstance(proxyType, realInstance1, proxyHandler);
        }
         
        public void Config(Action<ICachePolicyProvider<T>> expression)
        {
            expression(CachePolicyProvider);
        }
    }
}