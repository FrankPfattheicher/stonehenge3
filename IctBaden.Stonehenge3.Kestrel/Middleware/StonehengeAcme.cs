using System.IO;
using System.Threading.Tasks;
using IctBaden.Stonehenge3.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            var logger = context.Items["stonehenge.Logger"] as ILogger;
            
            var path = context.Request.Path.Value;
            if (path.StartsWith("/.well-known"))
            {
                var response = context.Response.Body;

                var rootPath = StonehengeApplication.BaseDirectory;
                var acmeFile = rootPath + context.Request.Path.Value;
                if (File.Exists(acmeFile))
                {
                    context.Response.Headers.Add("Cache-Control", new[] { "no-cache" });

                    var acmeData = await File.ReadAllBytesAsync(acmeFile);
                    await using var writer = new BinaryWriter(response);
                    writer.Write(acmeData);

                    return;
                }

                logger.LogError("No ACME data found.");
            }

            await _next.Invoke(context);
        }

    }
}
