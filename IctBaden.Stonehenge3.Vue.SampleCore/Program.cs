using System;
using System.Diagnostics;
using System.Threading;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.SimpleHttp;
using Microsoft.Extensions.Logging;

namespace IctBaden.Stonehenge3.Vue.SampleCore
{
    internal static class Program
    {
        private static IStonehengeHost _server;
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly ILoggerFactory LoggerFactory = StonehengeLogger.DefaultFactory;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
            StonehengeLogger.DefaultLevel = LogLevel.Trace;
            var logger = LoggerFactory.CreateLogger("stonehenge");

            Console.WriteLine(@"");
            Console.WriteLine(@"Stonehenge 3 sample");
            Console.WriteLine(@"");
            logger.LogInformation("Vue.SampleCore started");

            // select hosting options
            var options = new StonehengeHostOptions
            {
                Title = "VueSample",
                
                ServerPushMode = ServerPushModes.LongPolling,
                PollIntervalSec = 5,
                SessionIdMode = SessionIdModes.Automatic
                // SslCertificatePath = Path.Combine(StonehengeApplication.BaseDirectory, "stonehenge.pfx"),
                // SslCertificatePassword = "test"
            };

            // Select client framework
            Console.WriteLine(@"Using client framework vue");
            var vue = new VueResourceProvider(logger);
            var loader = StonehengeResourceLoader.CreateDefaultLoader(logger, vue);

            // Select hosting technology
            var hosting = "kestrel";
            if (Environment.CommandLine.Contains("/simple")) { hosting = "simple"; }
            switch (hosting)
            {
                case "kestrel":
                    Console.WriteLine(@"Using Kestrel hosting");
                    _server = new KestrelHost(loader, options);
                    break;
                case "simple":
                    Console.WriteLine(@"Using simple http hosting");
                    _server = new SimpleHttpHost(loader, options);
                    break;
            }

            Console.WriteLine(@"Starting server");
            var terminate = new AutoResetEvent(false);
            Console.CancelKeyPress += (_, _) => { terminate.Set(); };

            var host = Environment.CommandLine.Contains("/localhost") ? "localhost" : "*";
            if (_server.Start(host, 32000))
            {
                Console.WriteLine(@"Server reachable on: " + _server.BaseUrl);

                if (Environment.CommandLine.Contains("/window"))
                {
                    var wnd = new HostWindow(_server.BaseUrl, options.Title);
                    if (!wnd.Open())
                    {
                        logger.LogError("Failed to open main window");
                        terminate.Set();
                    }
                }
                else
                {
                    terminate.WaitOne();
                }
                Console.WriteLine(@"Server terminated.");
            }
            else
            {
                Console.WriteLine(@"Failed to start server on: " + _server.BaseUrl);
            }

#pragma warning disable 0162
            // ReSharper disable once HeuristicUnreachableCode
            _server.Terminate();
            
            Console.WriteLine(@"Exit sample app");
            Environment.Exit(0);
        }

    }
}
