using System;
using System.Linq.Expressions;

namespace DR.Sleipner.Config.Expressions
{
    public static class ExpressionConfigExtensions
    {
        public static IMethodFamilyConfigurationExpression For<T>(this ICachePolicyProvider<T> provider, Expression<Action<T>> expression) where T : class
        {
            var configuredMethod = new ExpressionConfiguredMethod<T>(expression);
            var policy = provider.RegisterMethodConfiguration(configuredMethod);

            return new MethodFamilyConfigExpression(policy);
        }

        public static IMethodFamilyConfigurationExpression DefaultIs<T>(this ICachePolicyProvider<T> provider) where T : class
        {
            var policy = provider.GetDefaultPolicy();

            return new MethodFamilyConfigExpression(policy);
        }
    }
}