using System;
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
            var parameterValues = proxyRequest.Parameters.Select(a => a is string ? "\"" + a + "\"" : a);

            var sb = new StringBuilder();
            sb.Append(typeof (T).FullName);
            sb.Append("-");
            sb.Append(proxyRequest.Method);
            sb.Append("-");
            sb.Append(string.Join(", ", parameterValues));

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hashAlgorithm = new SHA256Managed();
            var hash = hashAlgorithm.ComputeHash(bytes);
            
            return Convert.ToBase64String(hash);
        }
    }
}
