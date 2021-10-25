using System.Threading.Tasks;
using System.Web;
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
                var query = HttpUtility.ParseQueryString(context.Request.QueryString.ToString() ?? string.Empty);
                context.Response.Redirect($"/index.html?{query}");
                return;
            }

            await _next.Invoke(context);
        }
    }
}