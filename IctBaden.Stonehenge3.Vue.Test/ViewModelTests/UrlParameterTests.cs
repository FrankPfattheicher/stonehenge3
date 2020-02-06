using System;
using System.Diagnostics;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.ViewModelTests
{
    public class UrlParameterTests : IDisposable
        {
        private readonly VueTestApp _app;

        public UrlParameterTests()
        {
            _app = new VueTestApp();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void RequestWithParametersShouldSetSessionParameters()
        {
            var response = string.Empty;

            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableHttpClient())
                {
                    response = client.DownloadStringWithSession(_app.BaseUrl + "/ViewModel/StartVm?test=1234");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.NotNull(response);
            Assert.True(_app.Data.StartVmParameters.ContainsKey("test"));
            Assert.Equal("1234", _app.Data.StartVmParameters["test"]);
        }
        
        
    }
}