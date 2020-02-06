using System;
using System.Diagnostics;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.MultiApp
{
    public class DefaultAppTests : IDisposable
    {
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
                Debug.WriteLine(ex.Message);
            }

            Assert.NotNull(response);
            Assert.Contains("'start'", response);
            Assert.DoesNotContain("'secondapp'", response);
        }

    }
}
