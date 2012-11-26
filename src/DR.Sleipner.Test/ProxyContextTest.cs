using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProxy;
using DR.Sleipner.Test.TestModel;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class ProxyContextTest
    {
        [Test]
        public void GenericMethodMulti_string_int()
        {
            var methodName = "GenericMethodMulti";

            var instanceType = typeof(IAwesomeInterface);
            var parameters = new object[] { "", 0 };
            
            var context = new ProxyRequest<IAwesomeInterface, IDictionary<string, int>>(methodName, parameters);

            var methodInfo = instanceType.GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            methodInfo = methodInfo.MakeGenericMethod(typeof (string), typeof (int));

            Assert.AreEqual(context.Method, methodInfo);
            Assert.AreEqual(typeof(IDictionary<string, int>), context.Method.ReturnType);
        }

        [Test]
        public void GenericMethodMulti_string_string()
        {
            var methodName = "GenericMethodMulti";

            var instanceType = typeof(IAwesomeInterface);
            var parameters = new object[] { "", 0 };

            var context = new ProxyRequest<IAwesomeInterface, IDictionary<string, string>>(methodName, parameters);

            var methodInfo = instanceType.GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            methodInfo = methodInfo.MakeGenericMethod(typeof(string), typeof(string));

            Assert.AreEqual(context.Method, methodInfo);
            Assert.AreEqual(typeof(IDictionary<string, string>), context.Method.ReturnType);
        }

        [Test]
        public void GenericMethodsMustInitialize_Object()
        {
            var methodName = "GenericMethod";
            var instanceType = typeof (IAwesomeInterface);
            var parameters = new object[] {"", 0};

            var context = new ProxyRequest<IAwesomeInterface, IList<object>>(methodName, parameters);

            var methodInfo = instanceType.GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            methodInfo = methodInfo.MakeGenericMethod(typeof(object));

            Assert.AreEqual(context.Method, methodInfo);
            Assert.AreEqual(context.Method.ReturnType, typeof(IList<object>));
        }

        [Test]
        public void GenericMethodsMustInitialize_String()
        {
            var methodName = "GenericMethod";
            var instanceType = typeof(IAwesomeInterface);
            var parameters = new object[] { "", 0 };

            var context = new ProxyRequest<IAwesomeInterface, IList<int>>(methodName, parameters);

            var methodInfo = instanceType.GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            methodInfo = methodInfo.MakeGenericMethod(typeof(int));

            Assert.AreEqual(context.Method, methodInfo);
            Assert.AreEqual(context.Method.ReturnType, typeof(IList<int>));
        }

        [Test]
        public void GenericMethodsMustInitialize_Int()
        {
            var methodName = "GenericMethod";

            var instanceType = typeof(IAwesomeInterface);
            var parameters = new object[] { "", 0 };
            var context = new ProxyRequest<IAwesomeInterface, IList<string>>(methodName, parameters);

            var methodInfo = instanceType.GetMethod(methodName, parameters.Select(a => a.GetType()).ToArray());
            methodInfo = methodInfo.MakeGenericMethod(typeof(string));

            Assert.AreEqual(context.Method, methodInfo);
            Assert.AreEqual(context.Method.ReturnType, typeof(IList<string>));
        }
    }
}
