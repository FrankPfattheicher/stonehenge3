using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IctBaden.Stonehenge3.Kestrel.Middleware
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StonehengeRoot
    {
        private readonly RequestDelegate _next;

        // ReSharper disable once UnusedMember.Global
        public StonehengeRoot(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value.Replace("//", "/");
            if (path == "/")
            {
                context.Response.Redirect("Index.html");
                return;
            }

            await _next.Invoke(context);
        }
    }
}