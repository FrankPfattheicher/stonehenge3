using System;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Resources;

namespace IctBaden.Stonehenge3.Vue.Test
{
    public class VueTestApp : IDisposable
    {
        private const int Port = 7357;
        public string BaseUrl => _server?.BaseUrl;

        private readonly IStonehengeHost _server;

        public VueTestApp()
        {
            var loader = StonehengeResourceLoader.CreateDefaultLoader(new VueResourceProvider());
            var options = new StonehengeHostOptions
            {
                Title = "VueTest",
                StartPage = "start"
            };
            _server = new KestrelHost(loader);
            _server.Start("localhost", Port);
        }

        public void Dispose()
        {
            _server?.Terminate();
        }
    }
}
