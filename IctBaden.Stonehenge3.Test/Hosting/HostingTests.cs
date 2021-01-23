﻿using System;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Test.Tools;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Stonehenge3.Test.Hosting
{
    // ReSharper disable InconsistentNaming
    public class HostingTests
    {
        private readonly ILogger _logger = StonehengeLogger.DefaultLogger;

        [Fact]
        public void Host_StartupOk_RespondsOnHttpRequest()
        {
            const string content = "<h1>Test</h1>";

            var options = new StonehengeHostOptions
            {
                Title = "Test"
            };
            var loader = new TestResourceLoader(content);
            var host = new KestrelHost(loader, options);

            var startOk = host.Start("localhost", 32001);
            Assert.True(startOk, "Start failed");

            var response = string.Empty;
            try
            {
                using var client = new RedirectableWebClient();
                response = client.DownloadString(host.BaseUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(Host_StartupOk_RespondsOnHttpRequest));
            }

            Assert.Equal(content, response);
            host.Terminate();
        }

        [Fact]
        public void Host_MultipleInstances_StartupOk_RespondsOnHttpRequest()
        {
            const string content1 = "<h1>Test 01</h1>";
            const string content2 = "<h1>Test II</h1>";

            var options = new StonehengeHostOptions
            {
                Title = "Test"
            };
            var loader1 = new TestResourceLoader(content1);
            var host1 = new KestrelHost(loader1, options);

            var startOk = host1.Start("localhost", 32002);
            Assert.True(startOk, "Start host1 failed");

            var loader2 = new TestResourceLoader(content2);
            var host2 = new KestrelHost(loader2, options);

            startOk = host2.Start("localhost", 32003);
            Assert.True(startOk, "Start host2 failed");

            Assert.NotEqual(host1.BaseUrl, host2.BaseUrl);

            var response1 = string.Empty;
            var response2 = string.Empty;
            try
            {
                using (var client = new RedirectableWebClient())
                {
                    response1 = client.DownloadString(host1.BaseUrl);
                }
                using (var client = new RedirectableWebClient())
                {
                    response2 = client.DownloadString(host2.BaseUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(Host_MultipleInstances_StartupOk_RespondsOnHttpRequest));
                Assert.True(false, ex.Message);
            }

            Assert.Equal(content1, response1);
            Assert.Equal(content2, response2);

            host1.Terminate();
            host2.Terminate();
        }
    
    }
}
