using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DR.Sleipner.Config.Parsers;
using DR.Sleipner.Helpers;

namespace DR.Sleipner.Config.Expressions
{
    public class ExpressionConfiguredMethod<T> : IConfiguredMethod<T> where T : class
    {
        private readonly IEnumerable<IParameterParser> _parameterParsers;

        public MethodInfo Method;

        public ExpressionConfiguredMethod(Expression<Action<T>> expression)
        {
            var expressionBody = (MethodCallExpression)expression.Body;
            Method = expressionBody.Method;

            _parameterParsers = Parse(expressionBody).ToList();
        }

        public bool IsMatch(MethodInfo method, IEnumerable<object> arguments)
        {
            if (method != Method)
                return false;

            var args = arguments.ToArray();
            var parsers = _parameterParsers.ToArray();

            for (var i = 0; i < args.Length; i++)
            {
                var parser = parsers[i];
                var arg = args[i];

                if (!parser.IsMatch(arg))
                    return false;
            }

            return true;
        }

        private IEnumerable<IParameterParser> Parse(MethodCallExpression expression)
        {
            foreach (var argument in expression.Arguments)
            {
                if (argument is ConstantExpression)
                {
                    var constant = argument as ConstantExpression;
                    yield return new ConstantParser(constant.Value);
                }
                else if (argument is MethodCallExpression)
                {
                    var call = argument as MethodCallExpression;
                    var parserSelectors = new Func<IParameterParser>[]
                                              {
                                                  () => AnyValueParser(call),
                                                  () => BetweenParser(call),
                                                  () => new ConstantParser(SymbolExtensions.GetExpressionValue(call))
                                              };

                    var parser = parserSelectors.Select(a => a()).FirstOrDefault(a => a != null);
                    yield return parser;
                }
                else
                {
                    yield return new ConstantParser(SymbolExtensions.GetExpressionValue(argument));
                }
            }
        }

        private AnyValueParser AnyValueParser(MethodCallExpression call)
        {
            var methodInfo = call.Method;
            var isAnyMethod = typeof (Param).GetMethod("IsAny").MakeGenericMethod(methodInfo.ReturnType);

            if (methodInfo == isAnyMethod)
            {
                return new AnyValueParser();
            }

            return null;
        }

        private BetweenParser BetweenParser(MethodCallExpression call)
        {
            var methodInfo = call.Method;
            var betweenMethod = typeof(Param).GetMethods()[1].MakeGenericMethod(methodInfo.ReturnType);
            var slidingBetweenMethod = typeof(Param).GetMethods()[2].MakeGenericMethod(methodInfo.ReturnType);

            if (methodInfo == betweenMethod)
            {
                var parameters = call.Arguments.Select(a => (IComparable)SymbolExtensions.GetExpressionValue(a)).ToArray();

                return new BetweenParser(() => parameters[0], () => parameters[1]);
            }
            else if (methodInfo == slidingBetweenMethod)
            {
                var parameters = call.Arguments.Select(a => a as Expression<Func<IComparable>>).Select(a => a.Compile()).ToArray();

                return new BetweenParser(parameters[0], parameters[1]);
            }

            return null;
        }
    }
}