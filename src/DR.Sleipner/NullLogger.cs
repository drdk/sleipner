using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner
{
    public class NullLogger<T> : ILogger<T> where T : class
    {
        public void Log(LoggedTransaction transaction)
        {
        }
    }
}
