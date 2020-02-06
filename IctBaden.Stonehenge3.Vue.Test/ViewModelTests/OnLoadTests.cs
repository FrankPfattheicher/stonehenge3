using System;
using System.Diagnostics;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.ViewModelTests
{
    public class OnLoadTests : IDisposable
        {
        private readonly VueTestApp _app;

        public OnLoadTests()
        {
            _app = new VueTestApp();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void OnLoadShouldBeCalledForStartVmAfterFirstCall()
        {
            var response = string.Empty;

            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableHttpClient())
                {
                    response = client.DownloadStringWithSession(_app.BaseUrl + "/ViewModel/StartVm");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.NotNull(response);
            Assert.Equal(1, _app.Data.StartVmOnLoadCalled);
            Assert.Single(_app.Data.StartVmParameters);
        }
        
        
    }
}