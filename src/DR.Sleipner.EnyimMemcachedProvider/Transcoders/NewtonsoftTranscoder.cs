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
                TypeNameHandling = TypeNameHandling.All
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
                TypeNameHandling = TypeNameHandling.All
            };

            var data = new byte[item.Data.Count];
            Array.Copy(item.Data.Array, item.Data.Offset, data, 0, data.Length);

            using (var memoryStream = new MemoryStream(data))
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
                        return null;
                    }
                    catch(JsonSerializationException )
                    {
                        return null;
                    }
                }
            }
        }
    }
}
