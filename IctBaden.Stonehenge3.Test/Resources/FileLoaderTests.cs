﻿using System;
using System.Collections.Generic;
using System.IO;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Stonehenge3.Test.Resources
{
    public class FileLoaderTests : IDisposable
    {
        private readonly ILogger _logger = StonehengeLogger.DefaultLogger;
        private readonly FileLoader _loader;
        private readonly AppSession _session = new AppSession();
        private string _fullFileName;

        public FileLoaderTests()
        {
            var path = Path.GetTempPath();
            _loader = new FileLoader(_logger, path);
        }

        public void Dispose()
        {
            if ((_fullFileName != null) && File.Exists(_fullFileName))
            {
                File.Delete(_fullFileName);
            }
        }

        // ReSharper disable InconsistentNaming

        internal void CreateTextFile(string name)
        {
            _fullFileName = Path.Combine(_loader.RootPath, name);
            try
            {
                var file = File.CreateText(_fullFileName);
                file.Write("<!DOCTYPE html>" + Environment.NewLine + "<h1>Testfile</h1>");
                file.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CreateTextFile));
            }
        }

        internal void CreateBinaryFile(string name)
        {
            _fullFileName = Path.Combine(_loader.RootPath, name);
            try
            {
                var file = File.Create(_fullFileName);
                var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
                file.Write(data, 0, data.Length);
                file.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CreateBinaryFile));
            }
        }

        [Fact]
        public void Load_file_unknown_txt()
        {
            var resource = _loader.Get(_session, "unknown.txt", new Dictionary<string, string>());
            Assert.Null(resource);
        }

        [Fact]
        public void Load_file_icon_png()
        {
            var name = $"icon_{Guid.NewGuid():N}.png";
            CreateBinaryFile(name);
            var resource = _loader.Get(_session, name, new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("image/png", resource.ContentType);
            Assert.True(resource.IsBinary);
            Assert.Equal(16, resource.Data.Length);
        }

        [Fact]
        public void Load_file_index_html()
        {
            CreateTextFile("index.html");
            var resource = _loader.Get(_session, "index.html", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("text/html", resource.ContentType);
            Assert.False(resource.IsBinary);
            Assert.StartsWith("<!DOCTYPE html>", resource.Text);
        }

        [Fact]
        public void Load_file_image_png()
        {
            var name = $"image_{Guid.NewGuid():N}.jpg";
            CreateBinaryFile(name);
            var resource = _loader.Get(_session, name, new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("image/jpeg", resource.ContentType);
            Assert.True(resource.IsBinary);
            Assert.Equal(16, resource.Data.Length);
        }

        [Fact]
        public void Load_file_test_html()
        {
            CreateTextFile("test.htm");
            var resource = _loader.Get(_session, "test.htm", new Dictionary<string, string>());
            Assert.NotNull(resource);
            Assert.Equal("text/html", resource.ContentType);
            Assert.False(resource.IsBinary);
            Assert.StartsWith("<!DOCTYPE html>", resource.Text);
        }

    }
}