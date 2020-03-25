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
// ReSharper disable ConvertToUsingDeclaration

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
        public Task Invoke(HttpContext context)
        {
            lock (_next)
            {
                return InvokeLocked(context);
            }
        }

        public async Task InvokeLocked(HttpContext context)
        {
            var path = context.Request.Path.Value;
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
                var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
                var parameters = queryString.AllKeys
                    .ToDictionary(key => key, key => queryString[key]);

                appSession?.SetParameters(parameters);
                
                Resource content = null;
                switch (requestVerb)
                {
                    case "GET":
                        appSession?.Accessed(cookies, false);
                        content = resourceLoader?.Get(appSession, resourceName, parameters);
                        if ((content != null) && (string.Compare(resourceName, "index.html", StringComparison.InvariantCultureIgnoreCase) == 0))
                        {
                            HandleIndexContent(context, content);
                        }
                        break;

                    case "POST":
                        appSession?.Accessed(cookies, true);
                        var body = new StreamReader(context.Request.Body).ReadToEndAsync().Result;

                        try
                        {
                            var formData = !string.IsNullOrEmpty(body)
                                ? JsonConvert.DeserializeObject<JObject>(body).AsJEnumerable().Cast<JProperty>()
                                    .ToDictionary(data => data.Name,
                                        data => Convert.ToString(data.Value, CultureInfo.InvariantCulture))
                                : new Dictionary<string, string>();

                            content = resourceLoader?.Post(appSession, resourceName, parameters, formData);
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null) ex = ex.InnerException;
                            Trace.TraceError(ex.Message);
                            Trace.TraceError(ex.StackTrace);
                            
                            var exResource = new JObject
                            {
                                ["Message"] = ex.Message, 
                                ["StackTrace"] = ex.StackTrace
                            };
                            content = new Resource(resourceName, "StonehengeContent.Invoke.POST", ResourceType.Json, JsonConvert.SerializeObject(exResource), Resource.Cache.None);
                        }
                        break;
                }

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
                Trace.TraceError($"StonehengeContent write response: {ex.Message}" + Environment.NewLine + ex.StackTrace);
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