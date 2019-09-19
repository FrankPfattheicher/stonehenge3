using System;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Resources;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace IctBaden.Stonehenge3.Vue.Test
{
    public class VueTestApp : IDisposable
    {
        public string BaseUrl => _server?.BaseUrl;

        private readonly IStonehengeHost _server;

        public VueTestApp()
        {
            var loader = StonehengeResourceLoader.CreateDefaultLoader(new VueResourceProvider());
            _server = new KestrelHost(loader);
            _server.Start("localhost");
        }

        public void Dispose()
        {
            _server?.Terminate();
        }
        
    }
}
