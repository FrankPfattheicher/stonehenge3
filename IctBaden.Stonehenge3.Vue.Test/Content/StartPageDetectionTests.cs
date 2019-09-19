using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.Content
{
    public class StartPageDetectionTests : IDisposable
    {
        private readonly VueTestApp _app;
        private readonly string _response;

        public StartPageDetectionTests()
        {
            _app = new VueTestApp();
            
            _response = string.Empty;
            try
            {
                using (var client = new RedirectableWebClient())
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
        public void FistPageShouldBeStartPage()
        {
            // detect empty path route
            // { path: '', name: '', title: 'Start', component: () => Promise.resolve(stonehengeLoadComponent('start')), visible: false },
            var startPage = new Regex(@"{ path: ''.*stonehengeLoadComponent\('(\w+)'\)").Match(_response);
            Assert.True(startPage.Success);
            Assert.Equal("start", startPage.Groups[1].Value);
        }
        
    }
}

