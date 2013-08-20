using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;

namespace DR.Sleipner.EnyimMemcachedProvider.Transcoders
{
    public class ZippedNewtonsoftTranscoder : ITranscoder 
    {
        public CacheItem Serialize(object value)
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All
            };


            using (var ms = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(ms, CompressionMode.Compress))
                {
                    using (var zipStreamWriter = new StreamWriter(compressionStream))
                    {
                        serializer.Serialize(zipStreamWriter, value);
                    }
                }

                var compressedBytes = ms.ToArray();

                return new CacheItem()
                {
                    Data = new ArraySegment<byte>(compressedBytes),
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
            try
            {
                using (var memoryStream = new MemoryStream(data))
                {
                    using (var zipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        using (var streamReader = new StreamReader(zipStream))
                        {
                            var textReader = new JsonTextReader(streamReader);
                            try
                            {
                                return jsonSerializer.Deserialize(textReader);
                            }
                            catch (JsonReaderException)
                            {
                                return null;
                            }
                            catch (JsonSerializationException)
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (InvalidDataException)
            {
                //If zipstream was invalid
                return null;
            }
        }
    }
}