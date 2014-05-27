﻿using System;
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
        private readonly T _realInstance;
        private readonly ICacheProvider<T> _cacheProvider;
        public T Object { get; private set; }

        private readonly IProxyHandler<T> _proxyHandler;
        public readonly ICachePolicyProvider<T> CachePolicyProvider;

        internal IList<IConfiguredMethod<T>> ConfiguredMethods = new List<IConfiguredMethod<T>>(); 

        public SleipnerProxy(T realInstance, ICacheProvider<T> cacheProvider)
        {
            if (!typeof (T).IsInterface)
            {
                throw new ArgumentException("T must be an interface", "realInstance");
            }

            var proxyType = CacheProxyGenerator.GetProxyType<T>();

            _realInstance = realInstance;
            _cacheProvider = cacheProvider;
            CachePolicyProvider = new BasicConfigurationProvider<T>();
            _proxyHandler = new ThrottledProxyHandler<T>(_realInstance, CachePolicyProvider, cacheProvider);
            
            Object = (T)Activator.CreateInstance(proxyType, _realInstance, _proxyHandler);
        }

        public void Config(Action<ICachePolicyProvider<T>> expression)
        {
            expression(CachePolicyProvider);
        }

        public TResult Update<TResult>(Expression<Func<T, TResult>> expression)
        {
            var methodInfo = SymbolExtensions.GetMethodInfo(expression);
            var parameters = SymbolExtensions.GetParameter(expression);
            var proxyExpression = new ProxyRequest<T, TResult>(methodInfo, parameters);

            var realResult = _proxyHandler.GetRealResult(proxyExpression);
            var cachePolicy = CachePolicyProvider.GetPolicy(proxyExpression.Method, proxyExpression.Parameters);

            if (cachePolicy != null && cachePolicy.CacheDuration > 0)
            {
                _cacheProvider.StoreItem(proxyExpression, cachePolicy, realResult);
            }
            
            return realResult;
        }

        public void Purge<TResult>(Expression<Func<T, TResult>> expression)
        {
            _cacheProvider.Purge(expression);
        }
    }
}