using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DR.Sleipner.Caching.Contract;
using DR.Sleipner.Core;

namespace DR.Sleipner.Caching
{
    public class LookupDispatcher<T> : IInterceptHandler<T> where T : class
    {
        private readonly T _realInstance;
        private readonly ILookupHandler<T> _lookupHandler;
        private readonly ICacheProvider<T> _cacheProvider;
        private readonly IPolicyProvider<T> _policyProvider;
        private readonly ILookupContextBuilder<T> _lookupContextBuilder;

        public LookupDispatcher(T realInstance, ILookupHandler<T> lookupHandler, ICacheProvider<T> cacheProvider, IPolicyProvider<T> policyProvider) : this(realInstance, lookupHandler, cacheProvider, policyProvider, new DefaultLookupContextBuilder<T>())
        {
        }

        public LookupDispatcher(T realInstance, ILookupHandler<T> lookupHandler, ICacheProvider<T> cacheProvider, IPolicyProvider<T> policyProvider, ILookupContextBuilder<T> lookupContextBuilder)
        {
            _realInstance = realInstance;
            _lookupHandler = lookupHandler;
            _cacheProvider = cacheProvider;
            _policyProvider = policyProvider;
            _lookupContextBuilder = lookupContextBuilder;
        }

        public TResult GetResult<TResult>(MethodInfo methodInfo, object[] parameters)
        {
            var lookupRequestContext = _lookupContextBuilder.CreateLookupContext<TResult>(_realInstance, _cacheProvider, _policyProvider, methodInfo, parameters);
            lookupRequestContext.CurrentCachedObject = _cacheProvider.GetItem(lookupRequestContext);

            return _lookupHandler.Handle(lookupRequestContext);
        }
    }
}