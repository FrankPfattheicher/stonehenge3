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
using Microsoft.AspNetCore.Http.Internal;

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
            var timer = new Stopwatch();
            timer.Start();

            var path = context.Request.Path.ToString();

            if (path.ToLower().Contains("/user/"))
            {
                Trace.TraceInformation($"Stonehenge3.Kestrel Begin USER {context.Request.Method} {path}");
                await _next.Invoke(context);
                Trace.TraceInformation($"Stonehenge3.Kestrel End USER {context.Request.Method} {path}");
                return;
            }
            
            var cookie = context.Request.Headers.FirstOrDefault(h => h.Key == "Cookie");
            var stonehengeId = context.Request.Query["stonehenge-id"];
            if (!string.IsNullOrEmpty(cookie.Value.ToString()))
            {
                // workaround for double stonehenge-id values in cookie - take the last one
                var match = new Regex("stonehenge-id=([a-f0-9A-F]+)", RegexOptions.RightToLeft)
                    .Match(cookie.Value.ToString());
                if (match.Success)
                {
                    stonehengeId = match.Groups[1].Value;
                }
            }

            Trace.TraceInformation($"Stonehenge3.Kestrel[{stonehengeId}] Begin {context.Request.Method} {path}");

            var appSessions = context.Items["stonehenge.AppSessions"] as List<AppSession>
                ?? new List<AppSession>();

            CleanupTimedOutSessions(appSessions);
            var session = appSessions.FirstOrDefault(s => s.Id == stonehengeId);
            if (string.IsNullOrEmpty(stonehengeId) || session == null)
            {
                // session not found - redirect to new session
                var resourceLoader = context.Items["stonehenge.ResourceLoader"] as StonehengeResourceLoader;
                session = NewSession(appSessions, context, resourceLoader);

                context.Response.Headers.Add("Set-Cookie",
                    session.SecureCookies
                        ? new[] { "stonehenge-id=" + session.Id, "Secure" }
                        : new[] { "stonehenge-id=" + session.Id });

                var options = (StonehengeHostOptions)context.Items["stonehenge.HostOptions"];
                var redirectUrl = "/index.html";
                if (options.AddUrlSessionParameter)
                {
                    redirectUrl += "?stonehenge-id=" + session.Id;
                }
                context.Response.Redirect(redirectUrl);

                var remoteIp = context.Connection.RemoteIpAddress;
                var remotePort = context.Connection.RemotePort;
                Trace.TraceInformation($"Stonehenge3.Kestrel[{stonehengeId}] From IP {remoteIp}:{remotePort} - redirect to {session.Id}");
                return;
            }

            var etag = context.Request.Headers["If-None-Match"];
            if (context.Request.Method == "GET" && etag == session.GetResourceETag(path))
            {
                Debug.WriteLine("ETag match.");
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
            }
            else
            {
                context.Items.Add("stonehenge.AppSession", session);
                await _next.Invoke(context);
            }

            timer.Stop();

            if (context.RequestAborted.IsCancellationRequested)
            {
                Trace.TraceWarning(
                    $"Stonehenge3.Kestrel[{stonehengeId}] Canceled {context.Request.Method}={context.Response.StatusCode} {path}, {timer.ElapsedMilliseconds}ms");
                throw new TaskCanceledException();
            }

            Trace.TraceInformation(
                $"Stonehenge3.Kestrel[{stonehengeId}] End {context.Request.Method}={context.Response.StatusCode} {path}, {timer.ElapsedMilliseconds}ms");
        }

        private static void CleanupTimedOutSessions(ICollection<AppSession> appSessions)
        {
            var timedOutSessions = appSessions.Where(s => s.IsTimedOut).ToArray();
            foreach (var timedOut in timedOutSessions)
            {
                var vm = timedOut.ViewModel as IDisposable;
                vm?.Dispose();
                timedOut.ViewModel = null;
                appSessions.Remove(timedOut);
                Trace.TraceInformation($"Stonehenge3.Kestrel Session timed out {timedOut.Id}.");
            }
            Trace.TraceInformation($"Stonehenge3.Kestrel {appSessions.Count} sessions.");
        }

        private static AppSession NewSession(ICollection<AppSession> appSessions, HttpContext context, StonehengeResourceLoader resourceLoader)
        {
            var options = (StonehengeHostOptions)context.Items["stonehenge.HostOptions"];
            var session = new AppSession(resourceLoader, options);
            var isLocal = context.IsLocal();
            var userAgent = context.Request.Headers["User-Agent"];
            var httpContext = (context.Request as DefaultHttpRequest)?.HttpContext;
            var clientAddress = httpContext?.Connection.RemoteIpAddress.ToString();
            var hostDomain = context.Request.Host.Value;
            session.Initialize(hostDomain, isLocal, clientAddress, userAgent);
            appSessions.Add(session);
            Trace.TraceInformation($"Stonehenge3.Kestrel New session {session.Id}. {appSessions.Count} sessions.");
            return session;
        }
    }
}