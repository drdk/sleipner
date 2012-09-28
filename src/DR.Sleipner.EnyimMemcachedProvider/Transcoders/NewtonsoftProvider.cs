using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached;

namespace DR.Sleipner.EnyimMemcachedProvider.Transcoders
{
    public class NewtonsoftProvider : IProviderFactory<ITranscoder>
    {
        public ITranscoder Create()
        {
            return new NewtonsoftTranscoder();
        }

        public void Initialize(Dictionary<string, string> parameters)
        {
            //I have no idea what this does.
        }
    }
}
