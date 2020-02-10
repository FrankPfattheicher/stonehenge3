using System;
using System.Reflection;
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

        public readonly VueTestData Data = new VueTestData();

        public VueTestApp(Assembly appAssembly = null)
        {
            var vue = new VueResourceProvider();
            var loader = appAssembly != null
                ? StonehengeResourceLoader.CreateDefaultLoader(vue, appAssembly)
                : StonehengeResourceLoader.CreateDefaultLoader(vue);
            loader.Services.AddService(typeof(VueTestData), Data);
            _server = new KestrelHost(loader);
            _server.Start("localhost");
        }

        public void Dispose()
        {
            _server?.Terminate();
        }
        
    }
}
