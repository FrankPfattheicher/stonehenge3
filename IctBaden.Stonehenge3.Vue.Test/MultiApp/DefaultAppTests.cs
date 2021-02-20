using System;
using IctBaden.Stonehenge3.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.MultiApp
{
    public class DefaultAppTests : IDisposable
    {
        private readonly ILogger _logger = StonehengeLogger.DefaultLogger;
        private readonly VueTestApp _app;

        public DefaultAppTests()
        {
            _app = new VueTestApp();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void DefaultAppShouldContainPagesFromCurrentAssemblyOnly()
        {
            var response = string.Empty;
            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableHttpClient())
                {
                    response = client.DownloadStringWithSession(_app.BaseUrl + "/app.js");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(DefaultAppShouldContainPagesFromCurrentAssemblyOnly));
            }

            Assert.NotNull(response);
            Assert.Contains("'start'", response);
            Assert.DoesNotContain("'secondapp'", response);
        }

    }
}
