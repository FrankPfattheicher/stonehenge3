using System;
using System.Diagnostics;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.Content
{
    public class UserContentTests : IDisposable
    {
        private readonly VueTestApp _app;

        public UserContentTests()
        {
            _app = new VueTestApp();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void IndexShouldContainUserStylesRef()
        {
            var response = string.Empty;
            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableWebClient())
                {
                    response = client.DownloadString(_app.BaseUrl);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.NotNull(response);
            Assert.Contains("'styles/userstyles.css'", response);
        }

        [Fact]
        public void IndexShouldContainUserScriptsRef()
        {
            var response = string.Empty;
            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableWebClient())
                {
                    response = client.DownloadString(_app.BaseUrl);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.NotNull(response);
            Assert.Contains("'scripts/userscripts.js'", response);
        }

        [Fact]
        public void StartJsShouldContainStartUserScript()
        {
            var response = string.Empty;
            try
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using (var client = new RedirectableWebClient())
                {
                    response = client.DownloadStringWithSession(_app.BaseUrl + "/start.js");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.NotNull(response);
            Assert.Contains("'start_user_InitialLoaded'", response);
        }

    }
}
