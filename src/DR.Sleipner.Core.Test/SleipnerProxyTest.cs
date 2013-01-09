using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.TestHelpers;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Core.Test
{
    [TestFixture]
    public class SleipnerProxyTest
    {
        [Test]
        public void TestVoidMethod()
        {
            var interfaceMock = new Mock<ITestInterface>();
            var lookupHandlerMock = new Mock<IInterceptHandler<ITestInterface>>();

            var proxy = new SleipnerProxy<ITestInterface>(interfaceMock.Object, lookupHandlerMock.Object);

            proxy.Object.VoidMethod();

            interfaceMock.Verify(a => a.VoidMethod(), Times.Once());
        }

        [Test]
        public void TestParameterLessMethod()
        {
            var interfaceMock = new Mock<ITestInterface>();
            var lookupHandlerMock = new Mock<IInterceptHandler<ITestInterface>>();

            var proxy = new SleipnerProxy<ITestInterface>(interfaceMock.Object, lookupHandlerMock.Object);

            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, string>(a => a.ParameterLessMethod());
            var expectedResult = "OK-RESULT";

            lookupHandlerMock.Setup(a => a.GetResult<string>(call.Method, call.Parameters)).Returns(expectedResult);

            var actualResult = proxy.Object.ParameterLessMethod();

            Assert.AreSame(expectedResult, actualResult);
            lookupHandlerMock.Verify(a => a.GetResult<string>(call.Method, call.Parameters), Times.Once());
        }

        [Test]
        public void TestParameteredMethod()
        {
            var interfaceMock = new Mock<ITestInterface>();
            var lookupHandlerMock = new Mock<IInterceptHandler<ITestInterface>>();

            var proxy = new SleipnerProxy<ITestInterface>(interfaceMock.Object, lookupHandlerMock.Object);

            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 1));
            var expectedResult = new[] {1, 2, 3};

            lookupHandlerMock.Setup(a => a.GetResult<IEnumerable<int>>(call.Method, call.Parameters)).Returns(expectedResult);

            var actualResult = proxy.Object.ParameteredMethod("a", 1);

            Assert.AreSame(expectedResult, actualResult);
            lookupHandlerMock.Verify(a => a.GetResult<IEnumerable<int>>(call.Method, call.Parameters), Times.Once());
        }
    }
}