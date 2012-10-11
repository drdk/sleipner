using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.CacheProviders
{
    public abstract class CacheProviderBase<TImpl> where TImpl : class
    {
        /// <summary>
        /// This method is used to find the most specific cache behavior for a particular method info. The order of lookup is:
        /// 1) Concrete implementation of the method
        /// 2) The interface method definition
        /// 3) The class that implements the method
        /// 4) The interface definition
        /// </summary>
        /// <param name="methodInfo">The methodInfo (concrete implementing class as it's source) to start at</param>
        /// <returns>Cache behavior or exception</returns>
        protected CacheBehaviorAttribute GetCacheBehavior(MethodInfo methodInfo)
        {
            if(methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            var cacheAttribute = methodInfo.FindCacheBehavior();
            if (cacheAttribute == null)
            {
                throw new UnknownCacheBehaviorException();
            }

            return cacheAttribute;
        }

        protected string GenerateStringKey(MethodInfo methodInfo, object[] parameters)
        {
            var parameterValues = parameters.Select(a => a is string ? "\"" + a + "\"" : a);

            var sb = new StringBuilder();
            sb.Append(methodInfo.DeclaringType.FullName);
            sb.Append(".");
            sb.Append(methodInfo.Name);
            sb.Append("(");
            sb.Append(string.Join(", ", parameterValues));
            sb.Append(")");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hashAlgorithm = new SHA256Managed();
            var hash = hashAlgorithm.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}