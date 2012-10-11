using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Test.TestModel
{
    public abstract class AwesomeImplementation : IAwesomeInterface
    {
        public abstract void VoidMethod();
        public abstract IEnumerable<string> FaulyCachedMethod();
        public abstract IEnumerable<string> FaulyNonCachedMethod();
        public abstract IEnumerable<string> NonCachedMethod();
        public abstract IEnumerable<string> ParameterlessMethod();
        public abstract IEnumerable<string> ParameteredMethod(string a, int b);
        public abstract object LolMethod();
        public abstract int RoflMethod();
    }
}
