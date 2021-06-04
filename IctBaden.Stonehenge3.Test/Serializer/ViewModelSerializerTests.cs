using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IctBaden.Stonehenge3.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using JsonSerializer = IctBaden.Stonehenge3.ViewModel.JsonSerializer;
// ReSharper disable LocalFunctionCanBeMadeStatic
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace IctBaden.Stonehenge3.Test.Serializer
{
    public class ViewModelSerializerTests
    {
        private readonly ILogger _logger = StonehengeLogger.DefaultLogger;
        
        [Fact]
        public void SimpleClassSerializationShouldWork()
        {
            var model = new SimpleClass
            {
                Integer = 5,
                FloatingPoint = 1.23,
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

            Assert.Contains("FloatingPoint", json);
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
            //TODO   
        }

        [Fact]
        public void SerializerShouldRespectCustomSerializers()
        {
            //TODO   
        }

        [Fact]
        public void NestedClassesSerializationShouldWork()
        {
            var simple = new SimpleClass
            {
                Integer = 5,
                FloatingPoint = 1.23,
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
                        NestedSimple = new[] {simple, simple, simple}
                    }
                }
            };


            var json = JsonSerializer.SerializeObjectString(null, model);

            var obj = JsonConvert.DeserializeObject(json);
            Assert.NotNull(obj);

            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void HierarchicalClassesSerializationShouldWork()
        {
            HierarchicalClass NewHierarchicalClass(string name, int depth)
            {
                return new HierarchicalClass
                {
                    Name = name,
                    Children = (depth > 0)
                        ? Enumerable.Range(1, 10)
                            .Select(ix => NewHierarchicalClass($"child {depth} {ix}", depth - 1))
                            .ToList()
                        : null
                };
            }

            var hierarchy = NewHierarchicalClass("Root", 3);
            
            var watch = new Stopwatch();
            watch.Start();
            
            var json = JsonSerializer.SerializeObjectString(null, hierarchy);

            watch.Stop();
            _logger.LogTrace($"HierarchicalClassesSerialization: {watch.ElapsedMilliseconds}ms");
            
            var obj = JsonConvert.DeserializeObject(json);
            Assert.NotNull(obj);

            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void DictionaryStringObjectSerializationShouldBeDoneAsObjects()
        {
            var dt = new DateTime(2020, 02, 12, 17, 37, 44, DateTimeKind.Utc);
            var dict = new Dictionary<string, object>
            {
                { "Integer", 5 },
                { "FloatingPoint", 1.23 },
                { "Text", "test" },
                { "Timestamp", dt }
            };
            
            var json = JsonSerializer.SerializeObjectString(null, dict);
 
            var obj = JsonConvert.DeserializeObject<JObject>(json);
            Assert.NotNull(obj);

            Assert.Equal(5, obj["Integer"].Value<int>());
            Assert.Equal(1.23, obj["FloatingPoint"].Value<double>());
            Assert.Equal("test", obj["Text"].Value<string>());
            Assert.Equal(dt, obj["Timestamp"].Value<DateTime>());
        }

        
    }
}