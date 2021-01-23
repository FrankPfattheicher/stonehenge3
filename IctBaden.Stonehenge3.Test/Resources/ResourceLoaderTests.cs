
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Xunit;

namespace IctBaden.Stonehenge3.Test.Resources
{
    public class ResourceLoaderTests : IDisposable
    {
        private readonly ResourceLoader _loader;
        private readonly AppSession _session = new AppSession();

        public ResourceLoaderTests()
        {
            var assemblies = new List<Assembly>
                 {
                     Assembly.GetAssembly(typeof(ResourceLoader)),
                     Assembly.GetExecutingAssembly(),
                     Assembly.GetCallingAssembly()
                 }
                .Distinct()
                .ToList();
            _loader = new ResourceLoader(StonehengeLogger.DefaultLogger, assemblies, Assembly.GetCallingAssembly());
        }

        public void Dispose()
        {
        }

        // ReSharper disable InconsistentNaming

        [Fact]
        public void Load_resource_unknown_txt()
        {
            var resource = _loader.Get(_session, "unknown.txt", new Dictionary<string, string>());
            Assert.Null(resource);
        }

        [Fact]
        public void Load_resource_icon_png()
        {
            var resource = _loader.Get(_session, "icon.png", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("image/png", resource.ContentType);
            Assert.True(resource.IsBinary);
            Assert.Equal(201, resource.Data.Length);
        }

        [Fact]
        public void Load_resource_icon32_png()
        {
            var resource = _loader.Get(_session, "icon32.png", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("image/png", resource.ContentType);
            Assert.True(resource.IsBinary);
            Assert.Equal(354, resource.Data.Length);
        }

        [Fact]
        public void Load_resource_image_png()
        {
            var resource = _loader.Get(_session, "image.jpg", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("image/jpeg", resource.ContentType);
            Assert.True(resource.IsBinary);
            Assert.Equal(1009, resource.Data.Length);
        }

        [Fact]
        public void Load_resource_test_html()
        {
            var resource = _loader.Get(_session, "test.html", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("text/html", resource.ContentType);
            Assert.False(resource.IsBinary);
            Assert.StartsWith("<!DOCTYPE html>", resource.Text);

            resource = _loader.Get(_session, "TesT.HTML", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("text/html", resource.ContentType);
            Assert.False(resource.IsBinary);
            Assert.StartsWith("<!DOCTYPE html>", resource.Text);
        }

        [Fact]
        public void Load_resource_testscript_js()
        {
            var resource = _loader.Get(_session, "lib/testscript.js", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("text/javascript", resource.ContentType);
            Assert.False(resource.IsBinary);
            Assert.Contains("function Test()", resource.Text);
        }
    }
}
