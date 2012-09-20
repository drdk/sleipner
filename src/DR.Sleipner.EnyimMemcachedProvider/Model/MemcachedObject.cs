using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner.EnyimMemcachedProvider.Model
{
    public class MemcachedObject<TObject>
    {
        public TObject Object;
        public bool IsException;
        public DateTime Created;
    }
}