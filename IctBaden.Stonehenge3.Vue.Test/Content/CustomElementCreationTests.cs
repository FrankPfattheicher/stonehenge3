using System;
using System.Diagnostics;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.Content
{
    public class CustomElementCreationTests : IDisposable
    {
        private readonly VueTestApp _app;
        private readonly string _response;

        public CustomElementCreationTests()
        {
            _app = new VueTestApp();
            
            _response = string.Empty;
            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableHttpClient())
                {
                    _response = client.DownloadStringWithSession(_app.BaseUrl + "/app.js");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void AppJsShouldContainCustomElement1Definition()
        {
            Assert.Contains("'CustElem1'", _response);
        }

        [Fact]
        public void AppJsShouldContainCustomElement2Definition()
        {
            Assert.Contains("'CustElem2'", _response);
        }

        [Fact]
        public void AppJsShouldContainCustomElement1Parameter()
        {
            Assert.Contains("['one']", _response);
        }

        [Fact]
        public void AppJsShouldContainCustomElement2ParameterLists()
        {
            Assert.Contains("['one','two']", _response);
        }
        
    }
}

