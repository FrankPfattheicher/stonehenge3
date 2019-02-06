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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.SimpleHttp
{
    public class SimpleHttpHost : IStonehengeHost
    {
        public string AppTitle { get; private set; }
        public string BaseUrl { get; private set; }
        public StonehengeHostOptions Options { get; private set; }


        private SimpleHttpServer _server;
        private readonly IStonehengeResourceProvider _resourceLoader;
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
            Options = options;
            _sessionCache = cache;
        }

        public bool Start(string title, bool useSsl = false, string hostAddress = null, int hostPort = 0)
        {
            AppTitle = title;

            if(useSsl)
                throw new NotSupportedException("SSL not supported.");
            if (hostPort == 0) hostPort = 80;

            BaseUrl = "http://"
                + (hostAddress ?? "127.0.0.1")
                + ":" + hostPort;

            _server = new SimpleHttpServer(hostPort);
            _server.HandleGet += ServerOnHandleGet;
            _server.HandlePost += ServerOnHandlePost;

            _server.Start();
            return true;
        }

        public void Terminate()
        {
            _server?.Terminate();
        }

        private void ServerOnHandleGet(SimpleHttpProcessor httpProcessor)
        {
            // get session
            var sessionId = string.Empty;
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
                session = new AppSession();
                sessionId = session.Id;
                _sessionCache.Add(sessionId, session);
            }

            var header = new Dictionary<string, string> { { "Cookie", "StonehengeSession=" + sessionId } };

            if (httpProcessor.Url == "/")
            {
                httpProcessor.WriteRedirect("/Index.html", header);
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
            httpProcessor.WriteSuccess(content.ContentType, header);
            if (content.IsBinary)
            {
                httpProcessor.WriteContent(content.Data);
            }
            else
            {
                httpProcessor.WriteContent(content.Text);
            }
        }

        private void ServerOnHandlePost(SimpleHttpProcessor httpProcessor, StreamReader streamReader)
        {
            var resourceName = httpProcessor.Url.Substring(1);
            var queryPart = "";
            var queryIndex = resourceName.IndexOf("?", 0, StringComparison.InvariantCulture);
            if (queryIndex != -1)
            {
                queryPart = resourceName.Substring(queryIndex + 1);
                resourceName = resourceName.Substring(0, queryIndex);
            }
            var body = streamReader.ReadToEnd();
            var formData = JsonConvert.DeserializeObject<JObject>(body).AsJEnumerable().Cast<JProperty>()
                .ToDictionary(data => data.Name, data => Convert.ToString(data.Value, CultureInfo.InvariantCulture));

            var queryString = HttpUtility.ParseQueryString(queryPart);
            var paramObjects = queryString.AllKeys
                .ToDictionary(key => key, key => queryString[key]);
            var content = _resourceLoader.Post(new AppSession(), resourceName, paramObjects, formData);
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
