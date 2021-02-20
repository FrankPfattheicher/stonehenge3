using System;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Vue.TestApp2.ViewModels;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.MultiApp
{
    public class SecondAppTests : IDisposable
    {
        private readonly ILogger _logger = StonehengeLogger.DefaultLogger;
        private readonly VueTestApp _app;

        public SecondAppTests()
        {
            _app = new VueTestApp(typeof(SecondAppVm).Assembly);
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void SecondAppShouldContainPagesFromSecondAssemblyOnly()
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
                _logger.LogError(ex, nameof(SecondAppShouldContainPagesFromSecondAssemblyOnly));
            }

            Assert.NotNull(response);
            Assert.Contains("'secondapp'", response);
            Assert.DoesNotContain("'start'", response);
        }

    }
}
