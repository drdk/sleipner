using System;
using System.Collections.Generic;
using DR.Sleipner.Core.ProxyGenerators;

namespace DR.Sleipner.Core
{
    public static class ProxyGeneratorCache
    {
        private static readonly Dictionary<Type, Type> ProxyCache = new Dictionary<Type, Type>();
        private static readonly IProxyGenerator ProxyGenerator = new ILGenProxyGenerator();

        public static T GetProxy<T>(T realInstance, ILookupHandler<T> lookupHandler) where T : class
        {
            var proxy = GetProxyType<T>();
            return (T)Activator.CreateInstance(proxy, realInstance, lookupHandler);
        }

        public static Type GetProxyType<T>() where T : class
        {
            var interfaceType = typeof(T);

            if (!interfaceType.IsInterface)
            {
                throw new ProxyTypeMustBeInterfaceException();
            }

            Type proxyType;
            if(!ProxyCache.TryGetValue(interfaceType, out proxyType))
            {
                proxyType = ProxyGenerator.CreateProxy<T>();
                ProxyCache[interfaceType] = proxyType;
            }

            return proxyType;
        }
    }
}