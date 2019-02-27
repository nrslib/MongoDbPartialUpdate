using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDbPartialUpdateModel.UpdateBuilder;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MongoDbPartialUpdateModel.Tests
{
    [TestClass]
    public class DictionaryUpdateBuilderTest
    {
        [TestMethod]
        public void TestNonUpdate()
        {
            var original = new Dictionary<object, object>
            {
                {
                    "Key1-1", "Value1"
                    
                },
                {
                    "Key1-2", new Dictionary<string, string>
                    {
                        {
                            "key2-1", "value2-1"
                        }
                    }
                }
            };

            var after = new Dictionary<object, object>
            {
                {
                    "Key1-1", "Value1"

                },
                {
                    "Key1-2", new Dictionary<string, string>
                    {
                        {
                            "key2-1", "value2-1"
                        }
                    }
                }
            };

            var builder = new DictionaryUpdateDefinitionBuilder();
            var result = builder.BuildUpdateDefine<BsonDocument>("Test", original, after);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void TestInsert()
        {
            var original = new Dictionary<object, object>
            {
                {
                    "Key1-1", "Value1"

                }
            };

            var after = new Dictionary<object, object>
            {
                {
                    "Key1-1", "Value1"

                },
                {
                    "Key1-2", new Dictionary<string, string>
                    {
                        {
                            "key2-1", "value2-1"
                        }
                    }
                }
            };

            var builder = new DictionaryUpdateDefinitionBuilder();
            var result = builder.BuildUpdateDefine<BsonDocument>("Test", original, after);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void TestRemove()
        {
            var original = new Dictionary<object, object>
            {
                {
                    "Key1-1", "Value1"

                },
                {
                    "Key1-2", new Dictionary<string, string>
                    {
                        {
                            "key2-1", "value2-1"
                        }
                    }
                }
            };

            var after = new Dictionary<object, object>
            {
                {
                    "Key1-1", "Value1"

                },
            };

            var builder = new DictionaryUpdateDefinitionBuilder();
            var result = builder.BuildUpdateDefine<BsonDocument>("Test", original, after);
            Assert.IsTrue(result.Count == 1);
        }
    }
}
