using System.Collections.Generic;
using IctBaden.Stonehenge2.Core;
using IctBaden.Stonehenge2.Resources;
using IctBaden.Stonehenge3.Kestrel.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IctBaden.Stonehenge3.Kestrel
{
    public class Startup
    {
        public string _appTitle;
        public IStonehengeResourceProvider _resourceLoader;
        private readonly List<AppSession> _appSessions = new List<AppSession>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ServerExceptionLogger>();
            //TODO app.UseMiddleware<StonehengeAcme>();
            //TODO app.UseCompression();
            app.Use((context, next) =>
            {
                context.Items.Add("stonehenge.AppTitle", _appTitle);
                context.Items.Add("stonehenge.ResourceLoader", _resourceLoader);
                context.Items.Add("stonehenge.AppSessions", _appSessions);
                return next.Invoke();
            });
            app.UseMiddleware<StonehengeSession>();
            app.UseMiddleware<StonehengeHeaders>();
            app.UseMiddleware<StonehengeRoot>();
            app.UseMiddleware<StonehengeContent>();
        }
    }
}
