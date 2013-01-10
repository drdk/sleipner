using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DR.Sleipner.Caching.Contract;
using DR.Sleipner.Caching.Model;
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
        public void HandleTest_Fresh()
        {
            var testInterface = new Mock<ITestInterface>(MockBehavior.Strict);
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>(MockBehavior.Strict);
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>(MockBehavior.Strict);

            var expectedResult = new[] { 1, 2, 3 };
            var cachedObject = new CachedObject<IEnumerable<int>>(CachedObjectState.Fresh, expectedResult);
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 22));
            
            var lookupContext = new DefaultLookupContextBuilder<ITestInterface>().CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);
            lookupContext.CurrentCachedObject = cachedObject;

            var asyncHandler = new AsyncronousLookupHandler<ITestInterface>();
            var actualResult = asyncHandler.Handle(lookupContext);

            Assert.AreSame(expectedResult, actualResult);
            cacheProvider.Verify(a => a.GetItem(lookupContext), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(TestException))]
        public void HandleTest_Fresh_Exception()
        {
            var testInterface = new Mock<ITestInterface>(MockBehavior.Strict);
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>(MockBehavior.Strict);
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>(MockBehavior.Strict);

            var exception = new TestException();
            var cachedObject = new CachedObject<string>(CachedObjectState.Fresh, exception);
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, string>(a => a.ParameterLessMethod());

            var lookupContext = new DefaultLookupContextBuilder<ITestInterface>().CreateLookupContext<string>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);
            lookupContext.CurrentCachedObject = cachedObject;

            var asyncHandler = new AsyncronousLookupHandler<ITestInterface>();
            asyncHandler.Handle(lookupContext);
        }

        [Test]
        public void HandleTest_Stale()
        {
            var testInterface = new Mock<ITestInterface>(MockBehavior.Strict);
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>(MockBehavior.Strict);
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>(MockBehavior.Strict);

            var expectedResult = new[] { 1, 2, 3 };
            var newResult = new[] { 3, 2, 3 };
            var cachedObject = new CachedObject<IEnumerable<int>>(CachedObjectState.Stale, expectedResult);
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 22));

            var lookupContext = new DefaultLookupContextBuilder<ITestInterface>().CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);
            lookupContext.CurrentCachedObject = cachedObject;

            testInterface.Setup(a => a.ParameteredMethod("a", 22)).Returns(newResult);
            cacheProvider.Setup(a => a.StoreItem(lookupContext, newResult));

            var asyncHandler = new AsyncronousLookupHandler<ITestInterface>();
            var actualResult = asyncHandler.Handle(lookupContext);
            Assert.AreSame(expectedResult, actualResult);

            Thread.Sleep(500);
            testInterface.Verify(a => a.ParameteredMethod("a", 22), Times.Once());
            cacheProvider.Verify(a => a.StoreItem(lookupContext, newResult), Times.Once());
            cacheProvider.Verify(a => a.StoreException(lookupContext, It.IsAny<Exception>()), Times.Never());
        }

        [Test]
        public void HandleTest_Stale_Exception()
        {
            var testInterface = new Mock<ITestInterface>(MockBehavior.Strict);
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>(MockBehavior.Strict);
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>(MockBehavior.Strict);

            var expectedResult = new[] { 1, 2, 3 };
            var exception = new Exception("Lolexception");
            var cachedObject = new CachedObject<IEnumerable<int>>(CachedObjectState.Stale, expectedResult);
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 22));

            var lookupContext = new DefaultLookupContextBuilder<ITestInterface>().CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);
            lookupContext.CurrentCachedObject = cachedObject;

            testInterface.Setup(a => a.ParameteredMethod("a", 22)).Throws(exception);
            cacheProvider.Setup(a => a.StoreException(lookupContext, exception));

            var asyncHandler = new AsyncronousLookupHandler<ITestInterface>();
            var actualResult = asyncHandler.Handle(lookupContext);
            Assert.AreSame(expectedResult, actualResult);

            Thread.Sleep(500);
            testInterface.Verify(a => a.ParameteredMethod("a", 22), Times.Once());
            cacheProvider.Verify(a => a.StoreItem(lookupContext, It.IsAny<IEnumerable<int>>()), Times.Never());
            cacheProvider.Verify(a => a.StoreException(lookupContext, exception), Times.Once());
        }

        [Test]
        public void HandleTest_None()
        {
            var testInterface = new Mock<ITestInterface>(MockBehavior.Strict);
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>(MockBehavior.Strict);
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>(MockBehavior.Strict);

            var newResult = new[] { 1, 2, 3 };
            var cachedObject = new CachedObject<IEnumerable<int>>(CachedObjectState.None, default(IEnumerable<int>));
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, IEnumerable<int>>(a => a.ParameteredMethod("a", 22));

            var lookupContext = new DefaultLookupContextBuilder<ITestInterface>().CreateLookupContext<IEnumerable<int>>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);
            lookupContext.CurrentCachedObject = cachedObject;

            testInterface.Setup(a => a.ParameteredMethod("a", 22)).Returns(newResult);
            cacheProvider.Setup(a => a.StoreItem(lookupContext, newResult));

            var asyncHandler = new AsyncronousLookupHandler<ITestInterface>();
            var actualResult = asyncHandler.Handle(lookupContext);
            Assert.AreSame(newResult, actualResult);

            testInterface.Verify(a => a.ParameteredMethod("a", 22), Times.Once());
            cacheProvider.Verify(a => a.StoreItem(lookupContext, newResult), Times.Once());
            cacheProvider.Verify(a => a.StoreException(lookupContext, It.IsAny<Exception>()), Times.Never());
        }

        [Test]
        public void HandleTest_None_Exception()
        {
            var testInterface = new Mock<ITestInterface>(MockBehavior.Strict);
            var cacheProvider = new Mock<ICacheProvider<ITestInterface>>(MockBehavior.Strict);
            var policyProvider = new Mock<IPolicyProvider<ITestInterface>>(MockBehavior.Strict);

            var exception = new TestException();
            var cachedObject = new CachedObject<string>(CachedObjectState.None, default(string));
            var call = ExpressionHelpers.GetMethodCallDescription<ITestInterface, string>(a => a.ParameterLessMethod());

            var lookupContext = new DefaultLookupContextBuilder<ITestInterface>().CreateLookupContext<string>(testInterface.Object, cacheProvider.Object, policyProvider.Object, call.Method, call.Parameters);
            lookupContext.CurrentCachedObject = cachedObject;

            testInterface.Setup(a => a.ParameterLessMethod()).Throws(exception);
            cacheProvider.Setup(a => a.StoreException(lookupContext, exception));

            var asyncHandler = new AsyncronousLookupHandler<ITestInterface>();
            try
            {
                asyncHandler.Handle(lookupContext);
                Assert.Fail("Handle did not throw exception");
            }
            catch(TestException) { /*this is expected. Just go on*/ }
            catch
            {
                Assert.Fail("Handle did not throw proper exception");
            }

            testInterface.Verify(a => a.ParameterLessMethod(), Times.Once());
            cacheProvider.Verify(a => a.StoreException(lookupContext, exception), Times.Once());
        }
    }

    public class TestException : Exception
    {
    }
}