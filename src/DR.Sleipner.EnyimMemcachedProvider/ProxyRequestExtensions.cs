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
        public static string CreateStringRepresentation<T, TResult>(this ProxyRequest<T, TResult> proxyRequest) where T : class
        {
            var sb = new StringBuilder();
            sb.Append(typeof(T).FullName);
            sb.Append(" - ");
            sb.Append(proxyRequest.Method);
            sb.Append(" - ");
            sb.AddParameterRepresentations(proxyRequest.Parameters);

            return sb.ToString();
        }

        public static string CreateHash<T, TResult>(this ProxyRequest<T, TResult> proxyRequest) where T : class
        {
            var sb = new StringBuilder();
            sb.Append(typeof (T).FullName);
            sb.Append(" - ");
            sb.Append(proxyRequest.Method);
            sb.Append(" - ");
            sb.AddParameterRepresentations(proxyRequest.Parameters);

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hashAlgorithm = new SHA256Managed();
            var hash = hashAlgorithm.ComputeHash(bytes);
            
            return Convert.ToBase64String(hash);
        }

        public static void AddParameterRepresentations(this StringBuilder builder, object value)
        {
            if (value == null)
            {
                builder.Append(".null");
            }
            else if (value is string)
            {
                builder.Append("\"" + value + "\"");
            }
            else if (value is IEnumerable)
            {
                var ienum = (IEnumerable)value;
                var collection = ienum.Cast<object>().ToArray();
                builder.Append("[");
                for(var i = 0; i < collection.Length; i++)
                {
                    builder.AddParameterRepresentations(collection[i]);
                    if (i < collection.Length-1)
                    {
                        builder.Append(",");
                    }
                    
                }
                builder.Append("]");
            }
            else
            {
                builder.Append(value);
            }
        }
    }
}
