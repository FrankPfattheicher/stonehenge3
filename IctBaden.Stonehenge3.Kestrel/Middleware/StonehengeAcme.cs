using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IctBaden.Stonehenge3.Kestrel.Middleware
{
    public class StonehengeAcme
    {
        private readonly RequestDelegate _next;

        // ReSharper disable once UnusedMember.Global
        public StonehengeAcme(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (path.StartsWith("/.well-known"))
            {
                var response = context.Response.Body;

                var rootPath = AppDomain.CurrentDomain.BaseDirectory;
                var acmeFile = rootPath + context.Request.Path.Value;
                if (File.Exists(acmeFile))
                {
                    context.Response.Headers.Add("Cache-Control", new[] { "no-cache" });

                    var acmeData = File.ReadAllBytes(acmeFile);
                    using (var writer = new BinaryWriter(response))
                    {
                        writer.Write(acmeData);
                    }

                    return;
                }

                Trace.TraceError("No ACME data found.");
            }

            await _next.Invoke(context);
        }

    }
}
