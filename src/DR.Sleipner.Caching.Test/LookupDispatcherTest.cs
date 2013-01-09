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
    public class LookupDispatcherTest
    {
        [Test]
        public void TestGetResult()
        {
            var testInterface = new Mock<ITestInterface>();
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>();
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>();
            var lookupController = new Mock<ILookupHandler<ITestInterface>>();
            var lookupContextBuilder = new Mock<ILookupContextBuilder<ITestInterface>>();

            var expectedResult = new [] {1, 2, 3};
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 22));
            var lookupContext = new LookupContext<ITestInterface, IEnumerable<int>>(
                testInterface.Object,
                cacheProvider.Object,
                policyProvider.Object,
                call.Method,
                call.Parameters);

            lookupController
                .Setup(a => a.Handle(lookupContext))
                .Returns(expectedResult);

            lookupContextBuilder
                .Setup(a => a.CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters))
                .Returns(lookupContext);

            var lookupDispatcher = new LookupDispatcher<ITestInterface>(testInterface.Object, lookupController.Object, cacheProvider.Object, policyProvider.Object, lookupContextBuilder.Object);
            var result = lookupDispatcher.GetResult<IEnumerable<int>>(call.Method, call.Parameters);

            Assert.AreSame(expectedResult, result);

            lookupContextBuilder.Verify(a => a.CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters));
            cacheProvider.Verify(a => a.GetItem(lookupContext), Times.Once());
        }
    }
}