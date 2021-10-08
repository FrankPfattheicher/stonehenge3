using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HttpMultipartParser;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

// ReSharper disable ConvertToUsingDeclaration

namespace IctBaden.Stonehenge3.Kestrel.Middleware
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StonehengeContent
    {
        private readonly RequestDelegate _next;
        private static readonly object LockViews = new object();
        private static readonly object LockEvents = new object();

        // ReSharper disable once UnusedMember.Global
        public StonehengeContent(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.Contains("/Events"))
            {
                lock (LockEvents)
                {
                    return InvokeLocked(context);
                }
            }

            lock (LockViews)
            {
                return InvokeLocked(context);
            }
        }

        private async Task InvokeLocked(HttpContext context)
        {
            var logger = context.Items["stonehenge.Logger"] as ILogger;
            var path = context.Request.Path.Value.Replace("//", "/");
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
                if ((appSession?.UseBasicAuth ?? false) && !CheckBasicAuthFromContext(appSession, context))
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    context.Response.Headers.Add("WWW-Authenticate", "Basic");
                    return;
                }

                if (appSession != null && string.IsNullOrEmpty(appSession.UserIdentity))
                {
                    appSession.SetUser(GetUserNameFromContext(context));
                }

                Resource content = null;
                switch (requestVerb)
                {
                    case "GET":
                        appSession?.Accessed(cookies, false);
                        content = resourceLoader?.Get(appSession, resourceName, parameters);
                        if (content == null && appSession != null &&
                            resourceName.EndsWith("index.html", StringComparison.InvariantCultureIgnoreCase))
                        {
                            logger.LogError(
                                $"Invalid path in index resource {resourceName} - redirecting to root index");
                            context.Response.Redirect("/index.html");
                            return;
                        }
                        else if (string.Compare(resourceName, "index.html",
                            StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            HandleIndexContent(context, content);
                        }

                        break;

                    case "POST":
                        appSession?.Accessed(cookies, true);

                        try
                        {
                            var body = new StreamReader(context.Request.Body).ReadToEndAsync().Result;
                            var formData = new Dictionary<string, string>();
                            if (!string.IsNullOrEmpty(body))
                            {
                                if (body.StartsWith("{"))
                                {
                                    try
                                    {
                                        var jsonData = JsonConvert.DeserializeObject<JObject>(body)
                                            .AsJEnumerable().Cast<JProperty>()
                                            .ToDictionary(data => data.Name,
                                                data => Convert.ToString(data.Value, CultureInfo.InvariantCulture));
                                        foreach (var kv in jsonData)
                                        {
                                            formData.Add(kv.Key, kv.Value);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        logger.LogWarning("Failed to parse post data as json");
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        await using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
                                        var parser = await MultipartFormDataParser.ParseAsync(bodyStream);
                                        foreach (var p in parser.Parameters)
                                        {
                                            formData.Add(p.Name, p.Data);
                                        }

                                        foreach (var f in parser.Files)
                                        {
                                            // Save temp file
                                            var fileName = Path.GetTempFileName();
                                            await using var file = File.OpenWrite(fileName);
                                            await f.Data.CopyToAsync(file);
                                            file.Close();
                                            formData.Add(f.Name, fileName);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        logger.LogWarning("Failed to parse post data as multipart form data");
                                    }
                                }
                            }

                            content = resourceLoader?.Post(appSession, resourceName, parameters, formData);
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null) ex = ex.InnerException;
                            logger.LogError(ex.Message);
                            logger.LogError(ex.StackTrace);

                            var exResource = new JObject
                            {
                                ["Message"] = ex.Message,
                                ["StackTrace"] = ex.StackTrace
                            };
                            content = new Resource(resourceName, "StonehengeContent.Invoke.POST", ResourceType.Json,
                                JsonConvert.SerializeObject(exResource), Resource.Cache.None);
                        }

                        break;
                }

                if (content == null)
                {
                    await _next.Invoke(context);
                    return;
                }

                context.Response.ContentType = content.ContentType;

                if (context.Items["stonehenge.HostOptions"] is StonehengeHostOptions {DisableClientCache: true})
                {
                    context.Response.Headers.Add("Cache-Control", new[] {"no-cache", "no-store", "must-revalidate", "proxy-revalidate"});
                    context.Response.Headers.Add("Pragma", new[] {"no-cache"});
                    context.Response.Headers.Add("Expires", new[] {"0"});
                }
                else
                {
                    switch (content.CacheMode)
                    {
                        case Resource.Cache.None:
                            context.Response.Headers.Add("Cache-Control", new[] {"no-cache"});
                            break;
                        case Resource.Cache.Revalidate:
                            context.Response.Headers.Add("Cache-Control",
                                new[] {"max-age=3600", "must-revalidate", "proxy-revalidate"});
                            var etag = appSession?.GetResourceETag(path);
                            context.Response.Headers.Add(HeaderNames.ETag, new StringValues(etag));
                            break;
                        case Resource.Cache.OneDay:
                            context.Response.Headers.Add("Cache-Control", new[] {"max-age=86400"});
                            break;
                    }
                }

                if (appSession is {StonehengeCookieSet: false} && appSession.HostOptions.AllowCookies)
                {
                    context.Response.Headers.Add("Set-Cookie",
                        appSession.SecureCookies
                            ? new[] {"stonehenge-id=" + appSession.Id, "Secure"}
                            : new[] {"stonehenge-id=" + appSession.Id});
                }

                if (content.IsNoContent)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NoContent;
                }
                else if (content.IsBinary)
                {
                    await using var writer = new StreamWriter(response);
                    await writer.BaseStream.WriteAsync(content.Data);
                }
                else
                {
                    await using var writer = new StreamWriter(response);
                    await writer.WriteAsync(content.Text);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"StonehengeContent write response: {ex.Message}" + Environment.NewLine + ex.StackTrace);
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    logger.LogError(" + " + ex.Message);
                }
            }
        }


        private bool CheckBasicAuthFromContext(AppSession appSession, HttpContext context)
        {
            var auth = context.Request.Headers["Authorization"].FirstOrDefault();
            if (auth == null) return false;

            if (auth.StartsWith("Basic ", StringComparison.InvariantCultureIgnoreCase))
            {
                if (auth == appSession.VerifiedBasicAuth) return true;

                var userPassword = Encoding.ASCII.GetString(Convert.FromBase64String(auth.Substring(6)));
                var usrPwd = userPassword.Split(':');
                if (usrPwd.Length != 2) return false;

                var user = usrPwd[0];
                var pwd = usrPwd[1];
                var isValid = appSession.Passwords.IsValid(user, pwd);
                appSession.VerifiedBasicAuth = isValid ? auth : null;
                return isValid;
            }

            return false;
        }

        private string GetUserNameFromContext(HttpContext context)
        {
            var identityName = context.User.Identity.Name;
            if (identityName != null) return identityName;

            var auth = context.Request.Headers["Authorization"].FirstOrDefault();
            if (auth != null)
            {
                if (auth.StartsWith("Basic ", StringComparison.InvariantCultureIgnoreCase))
                {
                    var userPassword = Encoding.ASCII.GetString(Convert.FromBase64String(auth.Substring(6)));
                    identityName = userPassword.Split(':').FirstOrDefault();
                }
                else if (auth.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
                {
                    var token = auth.Substring(7);
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                    identityName = jwtToken?.Subject;
                }

                return identityName;
            }
            
            var isLocal = context.IsLocal();
            if (!isLocal) return null;

            var explorers = Process.GetProcessesByName("explorer");
            if (explorers.Length == 1)
            {
                identityName = $"{Environment.UserDomainName}\\{Environment.UserName}";
                return identityName;
            }
            
            // RDP with more than one session: How to find app and session using request's client IP port

            return null;
        }

        private void HandleIndexContent(HttpContext context, Resource content)
        {
            const string placeholderAppTitle = "stonehengeAppTitle";
            var appTitle = context.Items["stonehenge.AppTitle"].ToString();
            content.Text = content.Text.Replace(placeholderAppTitle, appTitle);
        }
    }
}