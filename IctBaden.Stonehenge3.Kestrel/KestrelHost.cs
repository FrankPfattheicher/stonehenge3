using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                Trace.TraceInformation($"KestrelHost.Start({hostAddress}, {hostPort})");
                if (hostPort == 0)
                {
                    hostPort = Network.GetFreeTcpPort();
                }

                IPAddress kestrelAddress;
                var useSsl = File.Exists(_options.SslCertificatePath);
                if(!string.IsNullOrEmpty(_options.SslCertificatePath))
                {
                    if (useSsl)
                    {
                        Trace.TraceInformation("KestrelHost.Start: Using SSL using certificate " + _options.SslCertificatePath);
                    }
                    else
                    {
                        Trace.TraceError("KestrelHost.Start: NOT using SSL - certificate not found: " + _options.SslCertificatePath);
                    }
                }
                var protocol = useSsl ? "https" : "http";
                string httpSysAddress;
                switch (hostAddress)
                {
                    case null:
                    case "*":
                        kestrelAddress = IPAddress.Any;
                        httpSysAddress = $"{protocol}://+:{hostPort}";
                        BaseUrl = $"{protocol}://{IPAddress.Loopback}:{hostPort}";
                        break;
                    case "localhost":
                        kestrelAddress = IPAddress.Loopback;
                        httpSysAddress = $"{protocol}://{kestrelAddress}:{hostPort}";
                        BaseUrl = $"{protocol}://{kestrelAddress}:{hostPort}";
                        break;
                    default:
                        kestrelAddress = IPAddress.Parse(hostAddress);
                        httpSysAddress = $"{protocol}://{kestrelAddress}:{hostPort}";
                        BaseUrl = $"{protocol}://{kestrelAddress}:{hostPort}";
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
                    Trace.TraceInformation("KestrelHost.Start: Using HttpSys mode (NTLM authentication).");
                    builder = builder
                        .UseHttpSys(options =>
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
                    Trace.TraceInformation("KestrelHost.Start: Using Kestrel/Sockets mode.");
                    builder = builder
                        .UseSockets()
                        .UseKestrel(options =>
                        {
                            // ensure no connection limit
                            options.Limits.MaxConcurrentConnections = null;
                            options.Listen(kestrelAddress, hostPort, listenOptions =>
                            {
                                if (useSsl)
                                {
                                    listenOptions.UseHttps(
                                        _options.SslCertificatePath,
                                        _options.SslCertificatePassword);
                                }
                            });
                        });
                }

                if (Environment.OSVersion.Platform == PlatformID.Win32Windows
                    || Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    builder = EnableIIS(builder);
                }

                _webApp = builder.Build();

                _cancel = new CancellationTokenSource();
                _host = _webApp.RunAsync(_cancel.Token);

                Trace.TraceInformation("KestrelHost.Start: succeeded.");
            }
            catch (Exception ex)
            {
                if ((ex.InnerException is HttpListenerException {ErrorCode: 5}))
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

        // ReSharper disable once InconsistentNaming
        private static IWebHostBuilder EnableIIS(IWebHostBuilder builder)
        {
            Trace.TraceInformation("KestrelHost.Start: Enable hosting in IIS");
            return builder.UseIIS();
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