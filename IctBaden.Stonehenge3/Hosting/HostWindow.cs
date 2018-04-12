using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Windows;
using IctBaden.Stonehenge3.Hosting;

namespace IctBaden.Stonehenge2.Hosting
{
    public class HostWindow
    {
        public string Title { get; set; }
        public Point WindowSize { get; set; }
        public string HostUrl { get; set; }

        public static Point DefaultWindowSize
        {
            get
            {
                //var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                //if (screen.Width > 1024)
                //{
                //    screen.Width = 1024;
                //    screen.Height = 768;
                //}
                //else if (screen.Width > 800)
                //{
                //    screen.Width = 800;
                //    screen.Height = 600;
                //}
                //return new Point(screen.Width, screen.Height);
                return new Point(800, 600);
            }
        }

        public HostWindow(string title, string hostUrl)
            : this(title, DefaultWindowSize, hostUrl)
        { 
        }
        public HostWindow(string title, Point windowSize, string hostUrl)
        {
            Title = title;
            WindowSize = windowSize;
            HostUrl = hostUrl;
        }

        /// <summary>
        /// Open a UI window using an installed browser 
        /// in kino mode - if possible.
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dir = Directory.CreateDirectory(path);

            var opened = ShowWindowMidori(path) ||
                         ShowWindowEpiphany() ||
                         ShowWindowChrome(path) ||
                         ShowWindowInternetExplorer() ||
                         ShowWindowFirefox() ||
                         ShowWindowSafari();
            if(!opened)
            {
                Trace.TraceError("Could not create main window");
            }

            try
            {
                dir.Delete(true);
            }
            catch
            {
                // ignore
            }
            return opened;
        }

        private bool ShowWindowChrome(string path)
        {
            var cmd = Environment.OSVersion.Platform == PlatformID.Unix ? "chromium-browser" : "chrome.exe";
            var parameter = $"--app={HostUrl}/?title={HttpUtility.UrlEncode(Title)} --window-size={WindowSize.X},{WindowSize.Y} --disable-translate --user-data-dir=\"{path}\"";
            var ui = Process.Start(cmd, parameter);
            if (ui == null)
            {
                return false;
            }
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, HostUrl);
            ui.WaitForExit();
            return true;
        }

        
        private bool ShowWindowEpiphany()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return false;

            var parameter = $"{HostUrl}/?title={HttpUtility.UrlEncode(Title)}";
            var ui = Process.Start("epiphany", parameter);
            if (ui == null)
            {
                return false;
            }
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, HostUrl);
            ui.WaitForExit();
            return true;
        }

        private bool ShowWindowMidori(string path)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return false;

            var parameter = $"-e Navigationbar -c {path} -a {HostUrl}/?title={HttpUtility.UrlEncode(Title)}";
            var ui = Process.Start("midori", parameter);
            if (ui == null)
            {
                return false;
            }
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, HostUrl);
            ui.WaitForExit();
            return true;
        }

        private bool ShowWindowInternetExplorer()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return false;

            const string cmd = "iexplore.exe";
            var parameter = $"-private {HostUrl}/?title={HttpUtility.UrlEncode(Title)}";
            var ui = Process.Start(cmd, parameter);
            if (ui == null)
            {
                return false;
            }
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, HostUrl);
            ui.WaitForExit();
            return true;
        }

        private bool ShowWindowFirefox()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return false;

            const string cmd = "firefox.exe";
            var parameter = $"{HostUrl}/?title={HttpUtility.UrlEncode(Title)} -width {WindowSize.X} -height {WindowSize.Y}";
            var ui = Process.Start(cmd, parameter);
            if (ui == null)
            {
                return false;
            }
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, HostUrl);
            ui.WaitForExit();
            return true;
        }

        private bool ShowWindowSafari()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return false;

            const string cmd = "safari.exe";
            var parameter = $"-url {HostUrl}/?title={HttpUtility.UrlEncode(Title)} -width {WindowSize.X} -height {WindowSize.Y}";
            var ui = Process.Start(cmd, parameter);
            if (ui == null)
            {
                return false;
            }
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, HostUrl);
            ui.WaitForExit();
            return true;
        }

    }
}
