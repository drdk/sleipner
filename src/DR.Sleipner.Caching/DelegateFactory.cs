using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DR.Sleipner.Caching
{
    /// <summary>
    /// Kindly stolen from: http://kohari.org/2009/03/06/fast-late-bound-invocation-with-expression-trees/
    /// 
    /// Author claims that this is fast as all hell
    /// </summary>

    public delegate object LateBoundMethod(object target, object[] arguments);
    public static class DelegateFactory
    {
        private static readonly IDictionary<MethodInfo, LateBoundMethod> Cache = new Dictionary<MethodInfo,LateBoundMethod>();

        public static LateBoundMethod GetOrCreate(MethodInfo methodInfo)
        {
            LateBoundMethod method;
            if(!Cache.TryGetValue(methodInfo, out method))
            {
                method = CreateDelegate(methodInfo);
                Cache[methodInfo] = method;
            }

            return method; 
        }

        private static LateBoundMethod CreateDelegate(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call = Expression.Call(
                Expression.Convert(instanceParameter, method.DeclaringType),
                method,
                CreateParameterExpressions(method, argumentsParameter));

            Expression<LateBoundMethod> lambda = Expression.Lambda<LateBoundMethod>(
                Expression.Convert(call, typeof(object)),
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        private static UnaryExpression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
                Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType))
                .ToArray();
        }
    }
}