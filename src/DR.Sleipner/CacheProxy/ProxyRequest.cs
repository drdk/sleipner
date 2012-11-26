using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using DR.Sleipner.Helpers;

namespace DR.Sleipner.CacheProxy
{
    public class ProxyRequest<T, TResult> where T : class
    {
        public MethodInfo Method;
        public object[] Parameters;

        public ProxyRequest(MethodInfo methodInfo, object[] parameters)
        {
            Method = methodInfo;
            if (Method.DeclaringType != typeof(T))
            {
                throw new ArgumentException("Declaring type is not T", "methodInfo");
            }

            Parameters = parameters;
        } 

        public bool Equals(ProxyRequest<T, TResult> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Equals(Method, other.Method) && Parameters.SequenceEqual(other.Parameters);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Method != null ? Method.GetHashCode() : 0) * 397);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProxyRequest<T, TResult>) obj);
        }
    }

    public class ProxyRequest<T> where T : class
    {
        public static ProxyRequest<T, TResult> FromExpression<TResult>(Expression<Func<T, TResult>> expression)
        {
            var methodInfo = SymbolExtensions.GetMethodInfo(expression);
            var parameters = SymbolExtensions.GetParameter(expression);
            return new ProxyRequest<T, TResult>(methodInfo, parameters);
        } 
    }
}
