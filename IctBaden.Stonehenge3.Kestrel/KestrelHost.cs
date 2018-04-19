using System;
using System.Diagnostics;
using System.Net;
using IctBaden.Stonehenge2.Hosting;
using IctBaden.Stonehenge2.Resources;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace IctBaden.Stonehenge3.Kestrel
{
    public class KestrelHost : IStonehengeHost
    {
        private IWebHost _webApp;
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

                _webApp = WebHost.CreateDefaultBuilder(new string[0])
                    .UseStartup<Startup>()
                    .UseUrls(BaseUrl)
                    .Build();

                _webApp.Run();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException as HttpListenerException;
                if ((inner != null) && (inner.ErrorCode == 5))
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
                Trace.TraceError("KatanaHost.Start: " + message);
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
