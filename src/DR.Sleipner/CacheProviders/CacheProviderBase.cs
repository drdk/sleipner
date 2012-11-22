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
        public static string GenerateStringKey(MethodInfo methodInfo, object[] parameters)
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