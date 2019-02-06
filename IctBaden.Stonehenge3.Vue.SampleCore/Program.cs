using System;
using System.Diagnostics;
using System.Threading;
using IctBaden.Stonehenge3.Caching;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.SimpleHttp;

namespace IctBaden.Stonehenge3.Vue.SampleCore
{
    internal static class Program
    {
        private static IStonehengeHost _server;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //var consoleListener = new  ConsoleTraceListener { Filter = new EventTypeFilter(SourceLevels.All) };
            var consoleListener = new ConsoleTraceListener();
            Trace.Listeners.Add(consoleListener);

            Console.WriteLine(@"");
            Console.WriteLine(@"Stonehenge 3 sample");
            Console.WriteLine(@"");

            // Select client framework
            Console.WriteLine(@"Using client framework vue");
            var loader = StonehengeResourceLoader.CreateDefaultLoader();
            var vue = new VueResourceProvider();
            vue.InitProvider(loader, "VueSample", "start");

            // Select hosting technology
            var hosting = "kestrel";
            if (Environment.CommandLine.Contains("/simple")) { hosting = "simple"; }
            switch (hosting)
            {
                case "kestrel":
                    Console.WriteLine(@"Using Kestrel hosting");
                    _server = new KestrelHost(loader) { DisableSessionIdUrlParameter = true };
                    break;
                case "simple":
                    Console.WriteLine(@"Using simple http hosting");
                    _server = new SimpleHttpHost(loader, new MemoryCache());
                    break;
            }

            Console.WriteLine(@"Starting server");
            var terminate = new AutoResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) => { terminate.Set(); };

            var host = Environment.CommandLine.Contains("/localhost") ? "localhost" : "*";
            if (_server.Start("VueSample", false, host, 32000))
            {
                Console.WriteLine(@"Started server on: " + _server.BaseUrl);

                if (Environment.CommandLine.Contains("/window"))
                {
                    var wnd = new HostWindow(_server);
                    if (!wnd.Open())
                    {
                        Trace.TraceError("Failed to open main window.");
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
            Environment.Exit(0);
        }
    }
}
