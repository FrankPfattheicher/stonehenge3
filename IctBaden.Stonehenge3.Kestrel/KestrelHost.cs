using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace IctBaden.Stonehenge3.Kestrel
{
    public class KestrelHost : IStonehengeHost
    {
        private IWebHost _webApp;
        // ReSharper disable once NotAccessedField.Local
        private readonly IStonehengeResourceProvider _resourceLoader;

        public KestrelHost(IStonehengeResourceProvider loader)
        {
            _resourceLoader = loader;
        }

        public string AppTitle { get; private set; }

        public string BaseUrl { get; private set; }

        public bool Start(string title, bool useSsl, string hostAddress, int hostPort)
        {
            AppTitle = title;

            try
            {
                BaseUrl = (useSsl ? "https://" : "http://")
                          + (hostAddress ?? "*")
                          + ":"
                          + (hostPort != 0 ? hostPort : (useSsl ? 443 : 80));


                var mem = new MemoryConfigurationSource()
                {
                    InitialData = new[] 
                    {
                        new KeyValuePair<string, string>( "AppTitle", AppTitle)
                    }
                };

                var config = new ConfigurationBuilder()
                    .Add(mem)
                    .Build();

                _webApp = new WebHostBuilder()
                    .UseConfiguration(config)
                    .ConfigureServices(s => { s.AddSingleton<IConfiguration>(config); })
                    .ConfigureServices(s => { s.AddSingleton(_resourceLoader); })
                    .UseStartup<Startup>()
                    .UseKestrel()
                    .UseUrls(BaseUrl)
                    .Build();

                _webApp.Run();
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
            _webApp?.Dispose();
            _webApp = null;
        }
    }
}
