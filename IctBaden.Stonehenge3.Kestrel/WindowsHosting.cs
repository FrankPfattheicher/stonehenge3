using Microsoft.AspNetCore.Hosting;

namespace IctBaden.Stonehenge3.Kestrel
{
    public static class WindowsHosting
    {
        /// <summary>
        /// Host ASP.NET Core on Windows with IIS
        /// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2#enable-the-iisintegration-components
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>builder</returns>
        // ReSharper disable once InconsistentNaming
        public static IWebHostBuilder EnableIIS(IWebHostBuilder builder)
        {
            return builder
                .UseIIS()               // in-proc hosting
                .UseIISIntegration();   // out-of-proc hosting   
        }

    }
}