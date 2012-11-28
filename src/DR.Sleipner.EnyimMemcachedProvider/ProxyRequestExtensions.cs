using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.EnyimMemcachedProvider
{
    public static class ProxyRequestExtensions
    {
        public static string CreateHash<T, TResult>(this ProxyRequest<T, TResult> proxyRequest) where T : class
        {
            var parameterValues = proxyRequest.Parameters
                                              .Select(EscapeStrings)
                                              .Select(EscapeNulls)
                                              .Select(ExpandCollections);
                

            var sb = new StringBuilder();
            sb.Append(typeof (T).FullName);
            sb.Append("-");
            sb.Append(proxyRequest.Method);
            sb.Append("-");
            sb.Append(CreateStringRepresentation(parameterValues));

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hashAlgorithm = new SHA256Managed();
            var hash = hashAlgorithm.ComputeHash(bytes);
            
            return Convert.ToBase64String(hash);
        }

        private static string CreateStringRepresentation(IEnumerable<object> parameters)
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(", ", parameters));

            return sb.ToString();
        }

        private static object EscapeStrings(object value)
        {
            if (value is string)
            {
                return "\"" + value + "\"";
            }

            return value;
        } 

        private static object EscapeNulls(object value)
        {
            return value ?? "n-u-l-l";
        }

        private static object ExpandCollections(object value)
        {
            if (value is string) //strings are collections - we don't want to do this to them
                return value;

            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                var coll = enumerable.Cast<object>();
                var result = coll.Select(EscapeStrings).Select(EscapeNulls).Select(ExpandCollections).ToArray();
                return "[" + CreateStringRepresentation(result) + "]";
            }

            return value;
        } 
    }
}
