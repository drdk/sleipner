using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Config;

namespace DR.Sleipner.Test
{
    public static class TestExtensions
    {
        public static bool IsMatch<T, TResult>(this IConfiguredMethod<T> method, Expression<Func<T, TResult>> expression) where T : class
        {
            var request = ProxyRequest<T>.FromExpression(expression);
            return method.IsMatch(request.Method, request.Parameters);
        }
    }
}