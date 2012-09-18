using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DR.Sleipner.CacheProxy
{
    /// <summary>
    /// Kindly stolen from: http://kohari.org/2009/03/06/fast-late-bound-invocation-with-expression-trees/
    /// 
    /// Author claims that this is fast as all hell
    /// </summary>
    public static class DelegateFactory
    {
        public delegate object LateBoundMethod(object target, object[] arguments);

        public static LateBoundMethod Create(MethodInfo method)
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

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
                                                 Expression.Convert(
                                                     Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
        }
    }
}