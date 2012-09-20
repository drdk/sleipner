using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.Test.TestModel
{
    public interface IAwesomeInterface
    {
        void VoidMethod();

        [CacheBehavior(Duration = 10)]
        IEnumerable<string> FaulyCachedMethod();
        IEnumerable<string> FaulyNonCachedMethod();

        IEnumerable<string> NonCachedMethod();

        [CacheBehavior(Duration = 10)]
        IEnumerable<string> ParameterlessMethod();

        [CacheBehavior(Duration = 10)]
        IEnumerable<string> ParameteredMethod(string a, int b);
    }
}
