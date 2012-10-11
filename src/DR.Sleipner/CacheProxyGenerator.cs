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

        public static T GetProxy<T, TImpl>(TImpl realInstance, ICacheProvider<TImpl> cacheProvider) where T : class where TImpl : class, T
        {
            var proxy = GetProxyType<T, TImpl>();
            return (T)Activator.CreateInstance(proxy, realInstance, cacheProvider);
        }

        public static Type GetProxyType<T, TImpl>() where T : class where TImpl : class, T
        {
            var interfaceType = typeof(T);
            var realType = typeof (TImpl);

            if (!interfaceType.IsInterface)
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
                proxyType = ProxyGenerator.CreateProxy<T, TImpl>();
                ProxyCache[realType] = proxyType;
            }

            return proxyType;
        }
    }
}
