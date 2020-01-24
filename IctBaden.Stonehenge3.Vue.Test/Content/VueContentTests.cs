using System;
using System.Diagnostics;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.Content
{
    /// <summary>
    /// Content that should be available as embedded resource
    /// </summary>
    public class VueContentTests : IDisposable
    {
        private readonly VueTestApp _app;

        public VueContentTests()
        {
            _app = new VueTestApp();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Theory]
        [InlineData("favicon.ico")]
        [InlineData("icon.png")]
        [InlineData("index.html")]
        [InlineData("src/bootstrap-grid.css")]
        [InlineData("src/bootstrap-grid.min.css")]
        [InlineData("src/bootstrap-reboot.css")]
        [InlineData("src/bootstrap-reboot.min.css")]
        [InlineData("src/bootstrap-vue.css")]
        [InlineData("src/bootstrap-vue.min.css")]
        [InlineData("src/bootstrap-vue.js")]
        [InlineData("src/bootstrap-vue.min.js")]
        [InlineData("src/bootstrap.css")]
        [InlineData("src/bootstrap.min.css")]
        [InlineData("src/bootstrap.min.js")]
        [InlineData("src/fontawesome_all.css")]
        [InlineData("src/fontawesome_all.min.css")]
        [InlineData("src/vue-resources.js")]
        [InlineData("src/vue-resources.min.js")]
        [InlineData("src/vue-router.js")]
        [InlineData("src/vue-router.min.js")]
        [InlineData("src/vue.js")]
        [InlineData("src/vue.min.js")]
        [InlineData("styles/styles.css")]
        [InlineData("webfonts/fa-brands-400.eot")]
        [InlineData("webfonts/fa-brands-400.svg")]
        [InlineData("webfonts/fa-brands-400.ttf")]
        [InlineData("webfonts/fa-brands-400.woff")]
        [InlineData("webfonts/fa-brands-400.woff2")]
        [InlineData("webfonts/fa-light-300.eot")]
        [InlineData("webfonts/fa-light-300.svg")]
        [InlineData("webfonts/fa-light-300.ttf")]
        [InlineData("webfonts/fa-light-300.woff")]
        [InlineData("webfonts/fa-light-300.woff2")]
        [InlineData("webfonts/fa-regular-400.eot")]
        [InlineData("webfonts/fa-regular-400.svg")]
        [InlineData("webfonts/fa-regular-400.ttf")]
        [InlineData("webfonts/fa-regular-400.woff")]
        [InlineData("webfonts/fa-regular-400.woff2")]
        [InlineData("webfonts/fa-solid-900.eot")]
        [InlineData("webfonts/fa-solid-900.svg")]
        [InlineData("webfonts/fa-solid-900.ttf")]
        [InlineData("webfonts/fa-solid-900.woff")]
        [InlineData("webfonts/fa-solid-900.woff2")]
        public void ShouldContainEmbeddedResourceContent(string content)
        {
            var response = string.Empty;
            var message = string.Empty;
            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableWebClient())
                {
                    response = client.DownloadString(_app.BaseUrl + "/" + content);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            Assert.True(response != null, message);
        }

    }
}