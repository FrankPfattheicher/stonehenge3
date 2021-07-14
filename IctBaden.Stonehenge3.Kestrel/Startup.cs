using System;
using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel.Middleware;
using IctBaden.Stonehenge3.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IctBaden.Stonehenge3.Kestrel
{
    public class Startup : IStartup
    {
        private readonly string _appTitle;
        private readonly ILogger _logger;
        private readonly IStonehengeResourceProvider _resourceLoader;
        public readonly List<AppSession> AppSessions = new List<AppSession>();
        private readonly StonehengeHostOptions _options;

        // ReSharper disable once UnusedMember.Global
        public Startup(ILogger logger, IConfiguration configuration, IStonehengeResourceProvider resourceLoader)
        {
            Configuration = configuration;
            _logger = logger;
            _resourceLoader = resourceLoader;
            _appTitle = Configuration["AppTitle"];
            _options = JsonConvert.DeserializeObject<StonehengeHostOptions>(Configuration["HostOptions"]);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // ReSharper disable once UnusedMember.Global
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.AddCors(o => o.AddPolicy("StonehengePolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ServerExceptionLogger>();
            app.UseMiddleware<StonehengeAcme>();
            app.UseResponseCompression();
            app.UseCors("StonehengePolicy");
            app.Use((context, next) =>
            {
                context.Items.Add("stonehenge.Logger", _logger);
                context.Items.Add("stonehenge.AppTitle", _appTitle);
                context.Items.Add("stonehenge.HostOptions", _options);
                context.Items.Add("stonehenge.ResourceLoader", _resourceLoader);
                context.Items.Add("stonehenge.AppSessions", AppSessions);
                return next.Invoke();
            });
            app.UseMiddleware<StonehengeSession>();
            app.UseMiddleware<StonehengeHeaders>();
            app.UseMiddleware<StonehengeRoot>();
            app.UseMiddleware<StonehengeContent>();
        }
    }
}
