using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace IctBaden.Stonehenge3.Kestrel
{
    public static class WindowsHosting
    {
        // ReSharper disable once InconsistentNaming
        public static IWebHostBuilder EnableIIS(IWebHostBuilder builder)
        {
            Trace.TraceInformation("KestrelHost.Start: Enable hosting in IIS");
            return builder.UseIIS();
        }

    }
}