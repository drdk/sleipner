using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DR.Sleipner.CacheProxy;

namespace DR.Sleipner.Test.TestModel
{
    public interface ILessAwesomeInterface
    {
        IList<T> GenericMethod<T>(string str, int number);
        void Rofl();
    }
}