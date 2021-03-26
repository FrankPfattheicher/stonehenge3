using System;
using IctBaden.Stonehenge3.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Stonehenge3.Vue.Test.ViewModelTests
{
    public class NotifyTests : IDisposable
    {
        private readonly ILogger _logger = StonehengeLogger.DefaultLogger;
        private readonly VueTestApp _app;

        public NotifyTests()
        {
            _app = new VueTestApp();
        }

        public void Dispose()
        {
            _app?.Dispose();
        }

        [Fact]
        public void ModifyNotifyPropertyShouldCreateEvent()
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
                _logger.LogError(ex, nameof(ModifyNotifyPropertyShouldCreateEvent));
            }

            Assert.NotNull(response);


            _app.Data.ExecAction("Notify");
            
            // rising event not yet tested ...
        }
        
        
    }
}