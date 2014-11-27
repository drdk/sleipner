using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DR.Sleipner.EnyimMemcachedProvider.Model;
using DR.Sleipner.EnyimMemcachedProvider.Transcoders;
using NUnit.Framework;

namespace DR.Sleipner.EnyimMemcachedProvider.Test
{
    [TestFixture]
    public class EnyimMemcachedProviderSerializationTests
    {
        [Test]
        public void TestSerializationOfTrickyExceptions()
        {
            var transcoder = new ZippedNewtonsoftTranscoder();
            var memObject = new MemcachedObject<string>();
            memObject.Created = DateTime.Now;
            memObject.Object = null;
            memObject.IsException = true;

            try
            {
                var bla = new SqlConnection("server=localhost;database=lolkalererer;uid=ffucktard;pwd=bøs!;application name=lawl;Connect Timeout=1");
                var command = new SqlCommand("EXECUTE NonExistantStoredProcedure", bla);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                memObject.Exception = e;
            }

            var cachedItem = transcoder.Serialize(memObject);
            var _memObj = transcoder.Deserialize(cachedItem);

            Assert.IsNull(_memObj);
        }
    }
}
