using System;
using System.Reflection;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.Vue;
using Microsoft.Extensions.Logging;

namespace IctBaden.Stonehenge3.App
{
    public class StonehengeUi : IDisposable
    {
        public IStonehengeHost Server;
        public readonly ILogger Logger;

        private StonehengeResourceLoader _loader;
        private StonehengeHostOptions _options;
        
        public StonehengeUi(string title)
        : this(new StonehengeHostOptions
        {
            Title = title,
            ServerPushMode = ServerPushModes.LongPolling,
            PollIntervalSec = 5,
            SessionIdMode = SessionIdModes.Automatic
        }, Assembly.GetEntryAssembly())
        {
        }

        public StonehengeUi(StonehengeHostOptions options, Assembly appAssembly)
        {
            _options = options;
            StonehengeLogger.DefaultLevel = LogLevel.Trace;
            Logger = StonehengeLogger.DefaultFactory.CreateLogger("stonehenge");

            var vue = new VueResourceProvider(Logger);
            _loader = StonehengeResourceLoader.CreateDefaultLoader(Logger, vue, appAssembly);
        }

        public void AddResourceAssembly(Assembly assembly)
        {
            _loader.AddResourceAssembly(assembly);
        }
        
        public bool Start() => Start(0, false);
        
        public bool Start(int port, bool publicReachable)
        {
            var host = publicReachable ? "*" : "localhost";
            Server = new KestrelHost(_loader, _options);
            return Server.Start(host, port);
        }

        public void Dispose()
        {
            Server.Terminate();
        }
        
    }
}