using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DR.Sleipner.Caching.Contract;

namespace DR.Sleipner.Caching
{
    public class DefaultLookupContextBuilder<T> : ILookupContextBuilder<T> where T : class 
    {
        public LookupContext<T, TResult> CreateLookupContext<TResult>(T realInstance, ICacheProvider<T> cacheProvider, IPolicyProvider<T> policyProvider, MethodInfo methodInfo, object[] parameters)
        {
            return new LookupContext<T, TResult>(realInstance, cacheProvider, policyProvider, methodInfo, parameters);
        }
    }
}
