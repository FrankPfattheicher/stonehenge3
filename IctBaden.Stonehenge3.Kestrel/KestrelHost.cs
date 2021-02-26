using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public KestrelHost(IStonehengeResourceProvider provider)
            : this(provider, new StonehengeHostOptions())
        {
        }

        public KestrelHost(IStonehengeResourceProvider provider, StonehengeHostOptions options)
            : this(provider, options, StonehengeLogger.DefaultLogger)
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public KestrelHost(IStonehengeResourceProvider provider, StonehengeHostOptions options, ILogger logger)
        {
            _resourceProvider = provider;
            _options = options;
            _logger = logger;

            provider.InitProvider(provider as StonehengeResourceLoader, options);
        }

        public bool Start(string hostAddress, int hostPort)
        {
            try
            {
                _logger.LogInformation($"KestrelHost.Start({hostAddress}, {hostPort})");
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
                        _logger.LogInformation("KestrelHost.Start: Using SSL using certificate " + _options.SslCertificatePath);
                    }
                    else
                    {
                        _logger.LogError("KestrelHost.Start: NOT using SSL - certificate not found: " + _options.SslCertificatePath);
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
                    .ConfigureServices(s => { s.AddSingleton(_logger); })
                    .ConfigureServices(s => { s.AddSingleton<IConfiguration>(config); })
                    .ConfigureServices(s => { s.AddSingleton(_resourceProvider); })
                    .UseStartup<Startup>();

                if (_options.UseNtlmAuthentication)
                {
                    _logger.LogInformation("KestrelHost.Start: Using HttpSys mode (NTLM authentication).");
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
                    _logger.LogInformation("KestrelHost.Start: Using Kestrel/Sockets mode.");
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

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // _logger.LogInformation("KestrelHost.Start: Enable hosting in IIS");
                    // builder = WindowsHosting.EnableIIS(builder);
                }

                _webApp = builder.Build();

                _cancel = new CancellationTokenSource();
                _host = _webApp.RunAsync(_cancel.Token);

                if (_host.IsFaulted)
                {
                    if (_host.Exception != null)
                    {
                        throw _host.Exception;
                    }
                }
                _logger.LogInformation("KestrelHost.Start: succeeded.");
            }
            catch (Exception ex)
            {
                if ((ex.InnerException is HttpListenerException {ErrorCode: 5}))
                {
                    _logger.LogError($"Access denied: Try netsh http delete urlacl {BaseUrl}");
                }
                else if (ex is MissingMemberException && ex.Message.Contains("Microsoft.Owin.Host.HttpListener"))
                {
                    _logger.LogError("Missing reference to nuget package 'Microsoft.Owin.Host.HttpListener'");
                }

                var message = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    message += Environment.NewLine + "    " + ex.Message;
                }

                _logger.LogError("KestrelHost.Start: " + message);
                _host.Dispose();
                _host = null;
            }

            return _host != null;
        }

        public void Terminate()
        {
            _logger.LogInformation("KestrelHost.Terminate: Cancel WebApp");
            _cancel?.Cancel();

            try
            {
                _logger.LogInformation("KestrelHost.Terminate: Host...");
                _host?.Wait();
                _host?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                _logger.LogInformation("KestrelHost.Terminate: WebApp...");
                _webApp?.Dispose();
                _webApp = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            _logger.LogInformation("KestrelHost.Terminate: Terminated.");
        }
        
        public void SetLogLevel(LogLevel level)
        {
            // TODO
        }

    }
}