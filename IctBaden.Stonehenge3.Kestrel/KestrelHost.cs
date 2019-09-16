using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Stonehenge3.Kestrel
{
    public class KestrelHost : IStonehengeHost
    {
        public string AppTitle => Options.Title;
        public string BaseUrl { get; private set; }
        public StonehengeHostOptions Options { get; private set; }

        
        private IWebHost _webApp;
        private Task _host;
        private CancellationTokenSource _cancel;
        private readonly IStonehengeResourceProvider _resourceProvider;

        public KestrelHost(IStonehengeResourceProvider provider)
        : this(provider, new StonehengeHostOptions())
        {
        }
        public KestrelHost(IStonehengeResourceProvider provider, StonehengeHostOptions options)
        {
            _resourceProvider = provider;
            Options = options;
            
            provider.InitProvider(provider as StonehengeResourceLoader, options);
        }

        public bool Start(string hostAddress, int hostPort)
        {
            try
            {
                IPAddress listenAddress;
                switch (hostAddress)
                {
                    case null:
                    case "*":
                        listenAddress = IPAddress.Any;
                        break;
                    case "localhost":
                        listenAddress = IPAddress.Loopback;
                        break;
                    default:
                        listenAddress = IPAddress.Parse(hostAddress);
                        break;
                }

                if (hostPort == 0)
                {
                    hostPort = Network.GetFreeTcpPort();
                }

                BaseUrl = $"http://{hostAddress}:{hostPort}";

                var mem = new MemoryConfigurationSource()
                {
                    InitialData = new[] 
                    {
                        new KeyValuePair<string, string>( "AppTitle", AppTitle),
                        new KeyValuePair<string, string>( "HostOptions", JsonConvert.SerializeObject(Options))
                    }
                };

                var config = new ConfigurationBuilder()
                    .Add(mem)
                    .Build();

                _webApp = new WebHostBuilder()
                    .UseConfiguration(config)
                    .ConfigureServices(s => { s.AddSingleton<IConfiguration>(config); })
                    .ConfigureServices(s => { s.AddSingleton(_resourceProvider); })
                    .UseSockets()
                    .UseStartup<Startup>()
                    .UseKestrel(options =>
                    {
                        // ensure no connection limit
                        options.Limits.MaxConcurrentConnections = null;
                        options.Listen(listenAddress, hostPort);
                    })
                    .Build();

                _cancel = new CancellationTokenSource();
                _host = _webApp.RunAsync(_cancel.Token);
            }
            catch (Exception ex)
            {
                if ((ex.InnerException is HttpListenerException inner) && (inner.ErrorCode == 5))
                {
                    Trace.TraceError($"Access denied: Try netsh http delete urlacl {BaseUrl}");
                }
                else if (ex is MissingMemberException && ex.Message.Contains("Microsoft.Owin.Host.HttpListener"))
                {
                    Trace.TraceError("Missing reference to nuget package 'Microsoft.Owin.Host.HttpListener'");
                }

                var message = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    message += Environment.NewLine + "    " + ex.Message;
                }
                Trace.TraceError("KestrelHost.Start: " + message);
                _webApp = null;
            }
            return _webApp != null;
        }

        public void Terminate()
        {
            Trace.TraceInformation("KestrelHost.Terminate: Cancel WebApp");
            _cancel?.Cancel();

            try
            {
                Trace.TraceInformation("KestrelHost.Terminate: Host...");
                _host?.Wait();
                _host?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                Trace.TraceInformation("KestrelHost.Terminate: WebApp...");
                _webApp?.Dispose();
                _webApp = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Trace.TraceInformation("KestrelHost.Terminate: Terminated.");
        }
    }
}
