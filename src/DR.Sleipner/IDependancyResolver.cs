using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner
{
    public interface IDependancyResolver
    {
        T Resolve<T>() where T : class;
    }
}
