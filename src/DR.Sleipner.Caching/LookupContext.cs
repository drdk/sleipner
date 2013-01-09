using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DR.Sleipner.Caching.Contract;
using DR.Sleipner.Caching.Model;

namespace DR.Sleipner.Caching
{
    public class LookupContext<T, TResult> where T : class
    {
        public ICacheProvider<T> CacheProvider;
        public IPolicyProvider<T> PolicyProvider;

        public MethodInfo Method;
        public object[] Parameters;

        private readonly LateBoundMethod _methodDelegate;
        public CachedObject<TResult> CurrentCachedObject;

        private readonly T _realInstance;

        public LookupContext(T realInstance, ICacheProvider<T> cacheProvider, IPolicyProvider<T> policyProvider, MethodInfo methodInfo, object[] parameters)
        {
            if (realInstance == null)
                throw new ArgumentNullException("cacheProvider");

            if(cacheProvider == null)
                throw new ArgumentNullException("cacheProvider");

            if (policyProvider == null)
                throw new ArgumentNullException("policyProvider");

            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            if (parameters == null)
                throw new ArgumentNullException("parameters");

            CacheProvider = cacheProvider;
            PolicyProvider = policyProvider;
            Method = methodInfo;
            Parameters = parameters;

            _realInstance = realInstance;
            _methodDelegate = DelegateFactory.GetOrCreate(methodInfo);
        }

        public TResult GetRealInstanceResult()
        {
            return (TResult)_methodDelegate(_realInstance, Parameters);
        }
    }
}