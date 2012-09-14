using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.CacheProxy.Generators;
using DR.Sleipner.CacheProxyCore;

namespace DR.Sleipner
{
    public static class CacheProxyGenerator
    {
        private static readonly Dictionary<Type, Type> ProxyCache = new Dictionary<Type, Type>();
        private static readonly IProxyGenerator ProxyGenerator = new ReflectionEmitProxyGenerator();

        public static T GetProxy<T>(T realInstance, ICacheProvider cacheProvider) where T : class
        {
            var realType = typeof (T);
            if(!realType.IsInterface)
            {
                throw new ProxyTypeMustBeInterfaceException();
            }
            
            Type proxyType;
            if (ProxyCache.ContainsKey(realType))
            {
                proxyType = ProxyCache[realType];
            }
            else
            {
                proxyType = ProxyGenerator.CreateProxy<T>();
                ProxyCache[realType] = proxyType;
            }
            
            return (T)Activator.CreateInstance(proxyType, realInstance, cacheProvider);
        }
    }
}
