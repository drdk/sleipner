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
    public class AsyncronousLookupHandlerTest
    {
        [Test]
        public void HandleTest()
        {
            var testInterface = new Mock<ITestInterface>();
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>();
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>();

            var expectedResult = new[] { 1, 2, 3 };
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 22));

            var lookupContext = new DefaultLookupContextBuilder<ITestInterface>().CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);
            
        }
    }
}
