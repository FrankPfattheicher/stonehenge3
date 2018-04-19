using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IctBaden.Stonehenge3.Kestrel.Middleware
{
    public class StonehengeRoot
    {
        private readonly RequestDelegate _next;

        public StonehengeRoot(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value == "/")
            {
                context.Response.Redirect("/Index.html");
                return;
            }

            await _next.Invoke(context);
        }
    }
}