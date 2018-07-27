using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Resources;
using Xunit;

namespace IctBaden.Stonehenge3.Test.Resources
{
    public class LoaderTests : IDisposable
    {
        private readonly StonehengeResourceLoader _loader;
        private readonly AppSession _session = new AppSession();

        private readonly FileLoaderTests _fileTest;

        public LoaderTests()
        {
            var assemblies = new List<Assembly>
                                 {
                                     Assembly.GetAssembly(typeof(ResourceLoader)),
                                     Assembly.GetExecutingAssembly()
                                 };
            var resLoader = new ResourceLoader(assemblies);
            var fileLoader = new FileLoader(Path.GetTempPath());

            _loader = new StonehengeResourceLoader(new List<IStonehengeResourceProvider>
            {
                fileLoader,
                resLoader
            });

            _fileTest = new FileLoaderTests();
        }

        public void Dispose()
        {
            _fileTest?.Dispose();
        }

        // ReSharper disable InconsistentNaming

        [Fact]
        public void Load_from_file_icon_png()
        {
            _fileTest.CreateBinaryFile("icon.png");
            var resource = _loader.Get(_session, "icon.png", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("image/png", resource.ContentType);
            Assert.True(resource.IsBinary);
            Assert.Equal(16, resource.Data.Length);
            Assert.StartsWith("file://", resource.Source);
        }

        [Fact]
        public void Load_from_resource_icon_png()
        {
            var resource = _loader.Get(_session, "image.jpg", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("image/jpeg", resource.ContentType);
            Assert.True(resource.IsBinary);
            Assert.Equal(1009, resource.Data.Length);
            Assert.StartsWith("res://", resource.Source);
        }

        [Fact]
        public void Load_from_file_over_resource_icon_png()
        {
            _fileTest.CreateTextFile("index.html");
            var resource = _loader.Get(_session, "index.html", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("text/html", resource.ContentType);
            Assert.False(resource.IsBinary);
            Assert.StartsWith("<!DOCTYPE html>", resource.Text);
            Assert.StartsWith("file://", resource.Source);
        }


    }
}