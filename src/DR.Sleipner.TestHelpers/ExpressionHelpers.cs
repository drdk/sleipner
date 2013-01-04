using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DR.Sleipner.TestHelpers
{
    public class ExpressionHelpers
    {
        public static MethodCallDescription GetMethodCallDescription<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            var callDescription = new MethodCallDescription()
                                      {
                                          Method = GetMethodInfo(expression),
                                          Parameters = GetParameter(expression)
                                      };

            return callDescription;
        }

        public static object[] GetParameter<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            var outermostExpression = expression.Body as MethodCallExpression;
            if (outermostExpression == null)
            {
                throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
            }

            var args = outermostExpression.Arguments.Select(a => GetExpressionValue(a)).ToArray();

            return args;
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            MethodCallExpression outermostExpression = expression.Body as MethodCallExpression;

            if (outermostExpression == null)
            {
                throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
            }

            return outermostExpression.Method;
        }

        public static object GetExpressionValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();

            return getter();
        }
    }
}
