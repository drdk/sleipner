using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.Caching.Contract;
using DR.Sleipner.Caching.Test.Contract;
using DR.Sleipner.TestHelpers;
using Moq;
using NUnit.Framework;

namespace DR.Sleipner.Caching.Test
{
    [TestFixture]
    public class DefaultLookupContextBuilderTest
    {
        [Test]
        public void LookupContextTest()
        {
            var testInterface = new Mock<ITestInterface>();
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>();
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>();

            var expectedResult = new [] {1, 2, 3};
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 22));

            var contextBuilder = new DefaultLookupContextBuilder<ITestInterface>();
            var createdContext = contextBuilder.CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);

            Assert.AreSame(cacheProvider.Object, createdContext.CacheProvider);
            Assert.AreSame(policyProvider.Object, createdContext.PolicyProvider);
            Assert.AreSame(call.Method, createdContext.Method);
            Assert.AreSame(call.Parameters, createdContext.Parameters);

            testInterface.Setup(a => a.ParameteredMethod("a", 22)).Returns(expectedResult);
            var result = createdContext.GetRealInstanceResult();
            Assert.AreSame(expectedResult, result);
            testInterface.Verify(a => a.ParameteredMethod(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
        }
    }
}
