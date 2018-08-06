using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using JsonSerializer = IctBaden.Stonehenge3.ViewModel.JsonSerializer;

namespace IctBaden.Stonehenge3.Test.Serializer
{
    public class ViewModelSerializerTests
    {
        [Fact]
        public void SimpleClassSerializonShouldWork()
        {
            var model = new SimpleClass
            {
                Integer = 5,
                Floatingpoint = 1.23,
                Text = "test",
                PrivateText = "invisible",
                Timestamp = new DateTime(2016, 11, 11, 12, 13, 14, DateTimeKind.Utc)
            };

            var json = JsonSerializer.SerializeObjectString(null, model);

            var obj = JsonConvert.DeserializeObject(json);
            Assert.NotNull(obj);

            // public properties - not NULL
            Assert.Contains("Integer", json);
            Assert.Contains("5", json);

            Assert.Contains("Boolean", json);
            Assert.Contains("false", json);

            Assert.Contains("Floatingpoint", json);
            Assert.Contains("1.23", json);

            Assert.Contains("Text", json);
            Assert.Contains("test", json);

            Assert.Contains("Timestamp", json);
            Assert.Contains("2016-11-11T12:13:14Z", json);

            // private fields
            Assert.DoesNotContain("PrivateText", json);
            Assert.DoesNotContain("invisible", json);
        }

        [Fact]
        public void StringsIncludingNewlineShouldBeEscaped()
        {
            var model = new SimpleClass
            {
                Text = "line1" + Environment.NewLine + "line2"
            };

            var json = JsonSerializer.SerializeObjectString(null, model);

            var obj = JsonConvert.DeserializeObject(json);
            Assert.NotNull(obj);

            Assert.Contains("\\n", json);
        }

        [Fact]
        public void SerializerShouldRespectAttributes()
        {
            
        }

        [Fact]
        public void SerializerShouldRespectCustomSerializers()
        {

        }

        [Fact]
        public void NestedClassesSerializonShouldWork()
        {
            var simple = new SimpleClass
            {
                Integer = 5,
                Floatingpoint = 1.23,
                Text = "test",
                PrivateText = "invisible",
                Timestamp = new DateTime(2016, 11, 11, 12, 13, 14, DateTimeKind.Utc)
            };

            var model = new NestedClass
            {
                //Name = "outer",
                Nested = new List<NestedClass2>
                {
                    new NestedClass2
                    {
                        NestedSimple = new[] { simple, simple, simple }
                    }
                }
            };
                
                
            var json = JsonSerializer.SerializeObjectString(null, model);

            var obj = JsonConvert.DeserializeObject(json);
            Assert.NotNull(obj);

            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

    }
}
