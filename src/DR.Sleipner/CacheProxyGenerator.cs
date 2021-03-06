﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using DR.Sleipner.CacheProviders;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.CacheProxy.Generators;

namespace DR.Sleipner
{
    public static class CacheProxyGenerator
    {
        private static readonly Dictionary<Type, Type> ProxyCache = new Dictionary<Type, Type>();
        private static readonly IProxyGenerator ProxyGenerator = new ILGenProxyGenerator();

        private static readonly object CreateProxyLock = new object();

        public static T GetProxy<T>(T realInstance, ICacheProvider<T> cacheProvider) where T : class
        {
            var proxy = GetProxyType<T>();
            return (T) Activator.CreateInstance(proxy, realInstance, cacheProvider);
        }

        public static Type GetProxyType<T>() where T : class
        {
            lock (CreateProxyLock)
            {
                var interfaceType = typeof (T);

                if (!interfaceType.IsInterface)
                {
                    throw new ProxyTypeMustBeInterfaceException();
                }

                Type proxyType;
                if (!ProxyCache.TryGetValue(interfaceType, out proxyType))
                {
                    proxyType = ProxyGenerator.CreateProxy<T>();
                    ProxyCache[interfaceType] = proxyType;
                }

                return proxyType;
            }
        }
    }
}