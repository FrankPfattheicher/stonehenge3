using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using IctBaden.Stonehenge3.Caching;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.SimpleHttp
{
    public class SimpleHttpHost : IStonehengeHost
    {
        public string BaseUrl { get; private set; }

        private SimpleHttpServer _server;
        private readonly IStonehengeResourceProvider _resourceLoader;
        // ReSharper disable once NotAccessedField.Local
        private readonly StonehengeHostOptions _options;
        private readonly IStonehengeSessionCache _sessionCache;

        public SimpleHttpHost(IStonehengeResourceProvider loader)
            : this(loader, new StonehengeHostOptions(), new MemoryCache())
        {
        }
        public SimpleHttpHost(IStonehengeResourceProvider loader, StonehengeHostOptions options)
            : this(loader, options, new MemoryCache())
        {
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public SimpleHttpHost(IStonehengeResourceProvider loader, StonehengeHostOptions options, IStonehengeSessionCache cache)
        {
            _resourceLoader = loader;
            _options = options;
            _sessionCache = cache;
            
            loader.InitProvider(null, options);
        }

        public bool Start(string hostAddress = null, int hostPort = 0)
        {
            if (hostPort == 0)
            {
                hostPort = Network.GetFreeTcpPort();
            }

            BaseUrl = "http://"
                + (hostAddress ?? "127.0.0.1")
                + ":" + hostPort;

            _server = new SimpleHttpServer(StonehengeLogger.DefaultLogger, hostPort);
            _server.HandleGet += ServerOnHandleGet;
            _server.HandlePost += ServerOnHandlePost;

            _server.Start();
            return true;
        }

        public void Terminate()
        {
            _server?.Terminate();
        }

        public void SetLogLevel(LogLevel level)
        {
            // TODO
        }

        private AppSession GetSession(SimpleHttpProcessor httpProcessor)
        {
            // get session
            string sessionId;
            AppSession session = null;
            var cookie = httpProcessor.Headers.FirstOrDefault(h => h.Key == "Cookie" && h.Value.Contains("StonehengeSession="));
            if(!string.IsNullOrEmpty(cookie.Value))
            {
                var extract = new Regex("StonehengeSession=([0-9a-fA-F]+)");
                var match = extract.Match(cookie.Value);
                sessionId = match.Groups[1].Value;
                if (_sessionCache.ContainsKey(sessionId))
                    session = _sessionCache[sessionId] as AppSession;
            }

            if (session == null)
            {
                var referer = httpProcessor.Headers.FirstOrDefault(h => h.Key == "Referer" && h.Value.Contains("stonehenge-id="));
                if (referer.Key != null)
                {
                    var id = new Regex("stonehenge-id=([0-9a-f]+)").Match(referer.Value);
                    if (id.Success)
                    {
                        sessionId = id.Groups[1].Value;
                        if (_sessionCache.ContainsKey(sessionId))
                            session = _sessionCache[sessionId] as AppSession;
                    }
                }
            }
            if (session == null)
            {
                session = new AppSession();
                sessionId = session.Id;
                _sessionCache.Add(sessionId, session);
            }

            return session;
        }
        
        private void ServerOnHandleGet(SimpleHttpProcessor httpProcessor)
        {
            var session = GetSession(httpProcessor);
            var header = new Dictionary<string, string> { { "Cookie", "StonehengeSession=" + session.Id } };

            if (httpProcessor.Url == "/")
            {
                httpProcessor.WriteRedirect("index.html?stonehenge-id=" + session.Id, header);
                return;
            }

            var resourceName = httpProcessor.Url.Substring(1);
            var parameters = new Dictionary<string, string>();  //TODO: extract parameters from URL
            var content = _resourceLoader.Get(session, resourceName, parameters);
            if (content == null)
            {
                httpProcessor.WriteNotFound();
                return;
            }
            if (content.IsBinary)
            {
                header.Add("Content-Length", content.Data.Length.ToString());
                httpProcessor.WriteSuccess(content.ContentType, header);
                httpProcessor.WriteContent(content.Data);
            }
            else
            {
                httpProcessor.WriteSuccess(content.ContentType, header);
                httpProcessor.WriteContent(content.Text);
            }
        }

        private void ServerOnHandlePost(SimpleHttpProcessor httpProcessor, StreamReader streamReader)
        {
            var resourceName = httpProcessor.Url.Substring(1);
            var body = streamReader.ReadToEnd();
            var formData = JsonConvert.DeserializeObject<JObject>(body).AsJEnumerable().Cast<JProperty>()
                .ToDictionary(data => data.Name, data => Convert.ToString(data.Value, CultureInfo.InvariantCulture));

            var queryString = HttpUtility.ParseQueryString(httpProcessor.Query);
            var paramObjects = queryString.AllKeys
                .ToDictionary(key => key, key => queryString[key]);

            var session = GetSession(httpProcessor);
            var content = _resourceLoader.Post(session, resourceName, paramObjects, formData);
            if (content == null)
            {
                httpProcessor.WriteNotFound();
                return;
            }
            httpProcessor.WriteSuccess(content.ContentType);
            if (content.IsBinary)
            {
                httpProcessor.WriteContent(content.Data);
            }
            else
            {
                httpProcessor.WriteContent(content.Text);
            }
        }

    }
}
