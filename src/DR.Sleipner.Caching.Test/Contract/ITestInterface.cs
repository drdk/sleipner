using System.Collections.Generic;

namespace DR.Sleipner.Caching.Test.Contract
{
    public interface ITestInterface
    {
        void VoidMethod();
        string ParameterLessMethod();
        IEnumerable<int> ParameteredMethod(string a, int b);
    }
}
