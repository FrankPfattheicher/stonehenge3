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
using AuthenticationSchemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Stonehenge3.Kestrel
{
    public class KestrelHost : IStonehengeHost
    {
        public string BaseUrl { get; private set; }
        private IWebHost _webApp;
        private Task _host;
        private CancellationTokenSource _cancel;

        private readonly IStonehengeResourceProvider _resourceProvider;
        private readonly StonehengeHostOptions _options;

        public KestrelHost(IStonehengeResourceProvider provider)
            : this(provider, new StonehengeHostOptions())
        {
        }

        public KestrelHost(IStonehengeResourceProvider provider, StonehengeHostOptions options)
        {
            _resourceProvider = provider;
            _options = options;

            provider.InitProvider(provider as StonehengeResourceLoader, options);
        }

        public bool Start(string hostAddress, int hostPort)
        {
            try
            {
                if (hostPort == 0)
                {
                    hostPort = Network.GetFreeTcpPort();
                }

                IPAddress kestrelAddress;
                string httpSysAddress;
                switch (hostAddress)
                {
                    case null:
                    case "*":
                        kestrelAddress = IPAddress.Any;
                        httpSysAddress = $"http://+:{hostPort}";
                        BaseUrl = $"http://{IPAddress.Loopback}:{hostPort}";
                        break;
                    case "localhost":
                        kestrelAddress = IPAddress.Loopback;
                        httpSysAddress = $"http://{kestrelAddress}:{hostPort}";
                        BaseUrl = $"http://{kestrelAddress}:{hostPort}";
                        break;
                    default:
                        kestrelAddress = IPAddress.Parse(hostAddress);
                        httpSysAddress = $"http://{kestrelAddress}:{hostPort}";
                        BaseUrl = $"http://{kestrelAddress}:{hostPort}";
                        break;
                }

                var mem = new MemoryConfigurationSource()
                {
                    InitialData = new[]
                    {
                        new KeyValuePair<string, string>("AppTitle", _options.Title),
                        new KeyValuePair<string, string>("HostOptions", JsonConvert.SerializeObject(_options))
                    }
                };

                var config = new ConfigurationBuilder()
                    .Add(mem)
                    .Build();

                var builder = new WebHostBuilder()
                    .UseConfiguration(config)
                    .ConfigureServices(s => { s.AddSingleton<IConfiguration>(config); })
                    .ConfigureServices(s => { s.AddSingleton(_resourceProvider); })
                    .UseStartup<Startup>();

                if (_options.UseNtlmAuthentication)
                {
                    builder = builder.UseHttpSys(options =>
                    {
                        // netsh http add urlacl url=https://+:32000/ user=TheUser
                        options.Authentication.Schemes =
                            (AuthenticationSchemes) (System.Net.AuthenticationSchemes.Ntlm |
                                                     System.Net.AuthenticationSchemes.Negotiate);
                        options.Authentication.AllowAnonymous = false;
                        options.UrlPrefixes.Add(httpSysAddress);
                    });
                }
                else
                {
                    builder = builder.UseSockets()
                        .UseKestrel(options =>
                        {
                            // ensure no connection limit
                            options.Limits.MaxConcurrentConnections = null;
                            options.Listen(kestrelAddress, hostPort);
                        });
                }


                _webApp = builder.Build();

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