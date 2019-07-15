using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IctBaden.Stonehenge3.Kestrel.Middleware
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StonehengeContent
    {
        private readonly RequestDelegate _next;

        // ReSharper disable once UnusedMember.Global
        public StonehengeContent(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var debug = 0;
            try
            {
                var response = context.Response.Body;
                var resourceLoader = context.Items["stonehenge.ResourceLoader"] as IStonehengeResourceProvider;
                var resourceName = path.Substring(1);
                var appSession = context.Items["stonehenge.AppSession"] as AppSession;
                var requestVerb = context.Request.Method;
                var cookiesHeader = context.Request.Headers
                    .FirstOrDefault(h => h.Key == HeaderNames.Cookie).Value.ToString();
                var requestCookies = cookiesHeader
                    .Split(';')
                    .Select(s => s.Trim())
                    .Select(s => s.Split('='));
                var cookies = new Dictionary<string, string>();
                foreach (var cookie in requestCookies)
                {
                    if (!cookies.ContainsKey(cookie[0]) && (cookie.Length > 1))
                    {
                        cookies.Add(cookie[0], cookie[1]);
                    }
                }
                debug = 10;
                var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
                debug = 11;
                var parameters = queryString.AllKeys
                    .ToDictionary(key => key, key => queryString[key]);
                debug = 12;

                Resource content = null;
                switch (requestVerb)
                {
                    case "GET":
                        debug = 13;
                        appSession?.Accessed(cookies, false);
                        debug = 14;
                        content = resourceLoader?.Get(appSession, resourceName, parameters);
                        debug = 15;
                        if ((content != null) && (string.Compare(resourceName, "index.html", StringComparison.InvariantCultureIgnoreCase) == 0))
                        {
                            HandleIndexContent(context, content);
                        }
                        break;

                    case "POST":
                        appSession?.Accessed(cookies, true);
                        var body = new StreamReader(context.Request.Body).ReadToEndAsync().Result;

                        var formData = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(body))
                        {
                            try
                            {
                                formData = JsonConvert.DeserializeObject<JObject>(body).AsJEnumerable().Cast<JProperty>()
                                .ToDictionary(data => data.Name, data => Convert.ToString(data.Value, CultureInfo.InvariantCulture));
                            }
                            catch (Exception ex)
                            {
                                if (ex.InnerException != null) ex = ex.InnerException;
                                Trace.TraceError(ex.Message);
                                Trace.TraceError(ex.StackTrace);
                                Debug.Assert(false);
                            }
                        }

                        content = resourceLoader?.Post(appSession, resourceName, parameters, formData);
                        break;
                }

                debug = 20;
                if (content == null)
                {
                    await _next.Invoke(context);
                    return;
                }
                context.Response.ContentType = content.ContentType;
                switch (content.CacheMode)
                {
                    case Resource.Cache.None:
                        context.Response.Headers.Add("Cache-Control", new[] { "no-cache" });
                        break;
                    case Resource.Cache.Revalidate:
                        context.Response.Headers.Add("Cache-Control", new[] { "max-age=3600", "must-revalidate", "proxy-revalidate" });
                        var etag = appSession?.GetResourceETag(path);
                        context.Response.Headers.Add(HeaderNames.ETag, new StringValues(etag));
                        break;
                    case Resource.Cache.OneDay:
                        context.Response.Headers.Add("Cache-Control", new[] { "max-age=86400" });
                        break;
                }
                if (appSession?.StonehengeCookieSet == false)
                {
                    context.Response.Headers.Add("Set-Cookie",
                        appSession.SecureCookies
                            ? new[] {"stonehenge-id=" + appSession.Id, "Secure"}
                            : new[] {"stonehenge-id=" + appSession.Id});
                }

                debug = 20;
                if (content.IsBinary)
                {
                    using (var writer = new BinaryWriter(response))
                    {
                        writer.Write(content.Data);
                    }
                }
                else
                {
                    using (var writer = new StreamWriter(response))
                    {
                        await writer.WriteAsync(content.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"StonehengeContent write response [{debug}]: {ex.Message}");
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Trace.TraceError(" + " + ex.Message);
                }
            }

        }

        private void HandleIndexContent(HttpContext context, Resource content)
        {
            const string placeholderAppTitle = "stonehengeAppTitle";
            var appTitle = context.Items["stonehenge.AppTitle"].ToString();
            content.Text = content.Text.Replace(placeholderAppTitle, appTitle);
        }
    }
}