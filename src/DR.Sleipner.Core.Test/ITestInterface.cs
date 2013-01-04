using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.Core.Test
{
    public interface ITestInterface
    {
        void VoidMethod();
        string ParameterLessMethod();
        IEnumerable<int> ParameteredMethod(string a, int b);
    }
}
