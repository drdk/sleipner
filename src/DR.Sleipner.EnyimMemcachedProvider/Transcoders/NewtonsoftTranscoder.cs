using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;

namespace DR.Sleipner.EnyimMemcachedProvider.Transcoders
{
    public class NewtonsoftTranscoder : ITranscoder 
    {
        public CacheItem Serialize(object value)
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    serializer.Serialize(writer, value);
                }
                return new CacheItem()
                {
                    Data = new ArraySegment<byte>(memoryStream.ToArray()),
                };
            }
        }

        public object Deserialize(CacheItem item)
        {
            var jsonSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            using (var memoryStream = new MemoryStream(item.Data.Array))
            {
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var textReader = new JsonTextReader(streamReader);
                    try
                    {
                        var dynamicEntity = jsonSerializer.Deserialize(textReader);

                        return dynamicEntity;
                    }
                    catch(JsonReaderException e)
                    {
                        return new object();
                    }
                }
            }
        }
    }
}
