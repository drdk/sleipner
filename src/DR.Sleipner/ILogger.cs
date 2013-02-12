using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner
{
    public interface ILogger<T> where T : class
    {
        void Log(LoggedTransaction transaction);
    }
}
