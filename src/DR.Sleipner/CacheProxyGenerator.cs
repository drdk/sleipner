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

        public static IDependancyResolver Resolver;

        public static T GetProxy<T, TImpl>() where T : class where TImpl : class, T
        {
            if(Resolver == null)
                throw new Exception("No resolver has been set");

            var instance = Resolver.Resolve<TImpl>();
            var cache = Resolver.Resolve<ICacheProvider<T>>();

            return GetProxy(instance, cache);
        }

        public static T GetProxy<T>(T realInstance, ICacheProvider<T> cacheProvider) where T : class
        {
            var proxy = GetProxyType<T>();
            return (T)Activator.CreateInstance(proxy, realInstance, cacheProvider);
        }

        public static Type GetProxyType<T>() where T : class
        {
            var realType = typeof(T);
            if (!realType.IsInterface)
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

            return proxyType;
        }
    }
}
