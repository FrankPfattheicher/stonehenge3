using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IctBaden.Stonehenge3.Kestrel.Middleware
{
    internal static class HttpContextExtensions
    {
        public static bool IsLocal(this HttpContext ctx)
        {
            var connection = ctx.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                else
                {
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StonehengeSession
    {
        private readonly RequestDelegate _next;

        // ReSharper disable once UnusedMember.Global
        public StonehengeSession(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext context)
        {
            var logger = context.Items["stonehenge.Logger"] as ILogger;
            
            var timer = new Stopwatch();
            timer.Start();

            var path = context.Request.Path.ToString();

            if (path.ToLower().Contains("/user/"))
            {
                logger.LogTrace($"Kestrel Begin USER {context.Request.Method} {path}");
                await _next.Invoke(context);
                logger.LogTrace($"Kestrel End USER {context.Request.Method} {path}");
                return;
            }

            var appSessions = context.Items["stonehenge.AppSessions"] as List<AppSession>
                              ?? new List<AppSession>();

            // URL id has priority
            var stonehengeId = context.Request.Query["stonehenge-id"];
            if (string.IsNullOrEmpty(stonehengeId))
            {
                var cookie = context.Request.Headers.FirstOrDefault(h => h.Key == "Cookie");
                if (!string.IsNullOrEmpty(cookie.Value.ToString()))
                {
                    // workaround for double stonehenge-id values in cookie - take the last one
                    var ids = new Regex("stonehenge-id=([a-f0-9A-F]+)", RegexOptions.RightToLeft)
                        .Matches(cookie.Value.ToString())
                        .Select(m => m.Groups[1].Value).ToArray();
                    if (ids.Length > 0)
                    {
                        stonehengeId = ids.FirstOrDefault(id => appSessions.Any(s => s.Id == id));
                    }
                }
            }

            logger.LogTrace($"Kestrel[{stonehengeId}] Begin {context.Request.Method} {path}{context.Request.QueryString}");

            CleanupTimedOutSessions(logger, appSessions);
            var session = appSessions.FirstOrDefault(s => s.Id == stonehengeId);
            if (session == null)
            {
                // session not found - redirect to new session
                var resourceLoader = context.Items["stonehenge.ResourceLoader"] as StonehengeResourceLoader;
                session = NewSession(logger, appSessions, context, resourceLoader);

                if (session.HostOptions.AllowCookies)
                {
                    context.Response.Headers.Add("Set-Cookie",
                        session.SecureCookies
                            ? new[] {"stonehenge-id=" + session.Id, "Secure"}
                            : new[] {"stonehenge-id=" + session.Id});
                }

                var redirectUrl = "/index.html";
                if (session.HostOptions.AddUrlSessionParameter)
                {
                    redirectUrl += "?stonehenge-id=" + session.Id;
                }

                context.Response.Redirect(redirectUrl);

                var remoteIp = context.Connection.RemoteIpAddress;
                var remotePort = context.Connection.RemotePort;
                logger.LogTrace($"Kestrel[{stonehengeId}] From IP {remoteIp}:{remotePort} - redirect to {session.Id}");
                return;
            }

            var etag = context.Request.Headers["If-None-Match"];
            if (context.Request.Method == "GET" && !string.IsNullOrEmpty(etag) && etag == session.GetResourceETag(path))
            {
                logger.LogTrace("ETag match.");
                context.Response.StatusCode = (int) HttpStatusCode.NotModified;
            }
            else
            {
                context.Items.Add("stonehenge.AppSession", session);
                await _next.Invoke(context);
            }

            timer.Stop();

            if (context.RequestAborted.IsCancellationRequested)
            {
                logger.LogTrace($"Kestrel[{stonehengeId}] Canceled {context.Request.Method}={context.Response.StatusCode} {path}, {timer.ElapsedMilliseconds}ms");
                throw new TaskCanceledException();
            }

            logger.LogTrace($"Kestrel[{stonehengeId}] End {context.Request.Method}={context.Response.StatusCode} {path}, {timer.ElapsedMilliseconds}ms");
        }

        private static void CleanupTimedOutSessions(ILogger logger, ICollection<AppSession> appSessions)
        {
            var timedOutSessions = appSessions.Where(s => s.IsTimedOut).ToArray();
            foreach (var timedOut in timedOutSessions)
            {
                var vm = timedOut.ViewModel as IDisposable;
                vm?.Dispose();
                timedOut.ViewModel = null;
                appSessions.Remove(timedOut);
                logger.LogInformation($"Kestrel Session timed out {timedOut.Id}.");
            }

            logger.LogInformation($"Kestrel {appSessions.Count} sessions.");
        }

        private static AppSession NewSession(ILogger logger, ICollection<AppSession> appSessions, HttpContext context,
            StonehengeResourceLoader resourceLoader)
        {
            var options = (StonehengeHostOptions) context.Items["stonehenge.HostOptions"];
            var session = new AppSession(resourceLoader, options);
            var isLocal = context.IsLocal();
            var userAgent = context.Request.Headers["User-Agent"];
            var httpContext = context.Request?.HttpContext;
            var clientAddress = httpContext?.Connection.RemoteIpAddress.ToString();
            var hostDomain = context.Request.Host.Value;
            session.Initialize(options, hostDomain, isLocal, clientAddress, userAgent);
            appSessions.Add(session);
            logger.LogInformation($"Kestrel New session {session.Id}. {appSessions.Count} sessions.");
            return session;
        }
    }
}