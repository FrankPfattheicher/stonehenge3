using System;
using System.Diagnostics;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.Components
{
    public class CustomElementCreationTests : IDisposable
    {
        private readonly VueTestApp _app;

        public CustomElementCreationTests()
        {
            _app = new VueTestApp();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void AppJsShouldContainCustomElementDefinitions()
        {
            var response = string.Empty;
            try
            {
                using (var client = new RedirectableWebClient())
                {
                    response = client.DownloadStringWithSession(_app.BaseUrl + "/app.js");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.NotNull(response);
            Assert.Contains("'CustElem1'", response);
            Assert.Contains("'CustElem2'", response);
        }

    }
}

