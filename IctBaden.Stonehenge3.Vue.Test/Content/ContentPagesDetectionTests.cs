using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.Content
{
    public class ContentPagesDetectionTests : IDisposable
    {
        private readonly VueTestApp _app;
        private readonly string _response;

        public ContentPagesDetectionTests()
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
        public void FistPageShouldBeStartPage()
        {
            // detect empty path route
            // { path: '', name: '', title: 'Start', component: () => Promise.resolve(stonehengeLoadComponent('start')), visible: false },
            var startPage = new Regex(@"{ path: ''.*stonehengeLoadComponent\('(\w+)'\)").Match(_response);
            Assert.True(startPage.Success);
            Assert.Equal("start", startPage.Groups[1].Value);
        }
        
        [Fact]
        public void HiddenPageShouldBeIncluded()
        {
            // { path: '/hidden', name: 'hidden', title: 'Hidden', component: () => Promise.resolve(stonehengeLoadComponent('hidden')), visible: false }
            var hiddenPage = new Regex(@"\{ path: \'/hidden'.*\}").Match(_response);
            Assert.True(hiddenPage.Success);
            var route = hiddenPage.Groups[0].Value;
            Assert.Contains("visible: false", route);
        }
        
    }
}

