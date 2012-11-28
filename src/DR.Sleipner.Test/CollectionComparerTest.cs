using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProxy;
using NUnit.Framework;

namespace DR.Sleipner.Test
{
    [TestFixture]
    public class CollectionComparerTest
    {
        [Test]
        public void TestCollectionComparison()
        {
            var a = new object[] { "a", "b", "c", new List<string> { "a", "a" } };
            var b = new object[] { "a", "b", "c", new List<string> { "a", "a" } };
            var c = new object[] { "a", "h", "c", new List<string> { "a", "a" } };
            var d = new object[] { "a", "c", "c", new List<string> { "a", "b" } };

            var aa = new object[] { "a", 1, "c", new List<int> { 2, 1 } };
            var bb = new object[] { "a", 1, "c", new List<int> { 2, 1 } };
            var cc = new object[] { "a", 1, "c", new List<int> { 2, 3 } };

            Assert.IsTrue(a.SequenceEqual(a, new CollectionComparer()), "A != A");
            Assert.IsTrue(a.SequenceEqual(b, new CollectionComparer()), "A != B");
            Assert.IsTrue(b.SequenceEqual(a, new CollectionComparer()), "B != A");
            Assert.IsTrue(b.SequenceEqual(b, new CollectionComparer()), "B != B");

            Assert.IsFalse(a.SequenceEqual(c, new CollectionComparer()), "A == C");
            Assert.IsFalse(a.SequenceEqual(d, new CollectionComparer()), "A == D");
            Assert.IsFalse(c.SequenceEqual(d, new CollectionComparer()), "C == D");

            Assert.IsTrue(aa.SequenceEqual(bb, new CollectionComparer()), "AA != BB");

            Assert.IsFalse(cc.SequenceEqual(bb, new CollectionComparer()), "CC == BB");
        }

        [Test]
        public void TestCollectionComparionNullSanity()
        {
            var comparer = new CollectionComparer();
            Assert.IsTrue(comparer.Equals(null, null), "Comparer dit not evaluate null and null as equal");
            Assert.IsFalse(comparer.Equals(null, new object()), "Comparer thinks null == new object()");
            Assert.IsFalse(comparer.Equals(new object(), null), "Comparer thinks new object() == null");
        }
    }
}
