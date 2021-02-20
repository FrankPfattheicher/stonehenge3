using Microsoft.AspNetCore.Hosting;

namespace IctBaden.Stonehenge3.Kestrel
{
    public static class WindowsHosting
    {
        // ReSharper disable once InconsistentNaming
        public static IWebHostBuilder EnableIIS(IWebHostBuilder builder)
        {
            return builder.UseIIS();
        }

    }
}