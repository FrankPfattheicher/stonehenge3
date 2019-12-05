using System;
using System.Diagnostics;
using IctBaden.Stonehenge3.Vue.TestApp2.ViewModels;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.MultiApp
{
    public class SecondAppTests : IDisposable
    {
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
            Assert.Contains("'secondapp'", response);
            Assert.DoesNotContain("'start'", response);
        }

    }
}
