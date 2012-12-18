using System.Collections.Generic;
using Enyim.Caching.Memcached;

namespace DR.Sleipner.EnyimMemcachedProvider.Transcoders
{
    public class ZippedNewtonsoftProvider : IProviderFactory<ITranscoder>
    {
        public ITranscoder Create()
        {
            return new ZippedNewtonsoftTranscoder();
        }

        public void Initialize(Dictionary<string, string> parameters)
        {
            //I have no idea what this does.
        }
    }
}