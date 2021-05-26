using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace IctBaden.Stonehenge3.Hosting
{
    public class HostWindow
    {
        private readonly string _title;
        private readonly Point _windowSize;
        private readonly string _startUrl;

        // ReSharper disable once MemberCanBePrivate.Global
        public string LastError;

        private static Point DefaultWindowSize
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

                // ReSharper disable once ArrangeAccessorOwnerBody
                return new Point(800, 600);
            }
        }

        public HostWindow(string startUrl)
            : this(startUrl, null, DefaultWindowSize)
        {
        }
        
        public HostWindow(string startUrl, string title)
            : this(startUrl, title, DefaultWindowSize)
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public HostWindow(string startUrl, string title, Point windowSize)
        {
            _startUrl = startUrl;
            _title = title ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "";
            _windowSize = windowSize;
        }

        /// <summary>
        /// Open a UI window using an installed browser 
        /// in kino mode - if possible.
        /// This method does not return until the window is closed.
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dir = Directory.CreateDirectory(path);

            var opened = ShowWindowMidori(path) ||
                         ShowWindowEpiphany() ||
                         ShowWindowChrome1(path) ||
                         ShowWindowChrome2(path) ||
                         ShowWindowEdge(path) ||
                         ShowWindowFirefox() ||
                         ShowWindowSafari() ||
                         ShowWindowInternetExplorer();
            if (!opened)
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

        private bool ShowWindowChrome1(string path)
        {
            try
            {
                var pi = new ProcessStartInfo
                {
                    FileName = Environment.OSVersion.Platform == PlatformID.Unix ? "chromium" : "chrome",
                    CreateNoWindow = true,
                    Arguments = $"--app={_startUrl}/?title={HttpUtility.UrlEncode(_title)} --window-size={_windowSize.X},{_windowSize.Y} --disable-translate --user-data-dir=\"{path}\"",
                    UseShellExecute = Environment.OSVersion.Platform != PlatformID.Unix
                };
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    pi.Arguments += " --disable-gpu";
                }
                var ui = Process.Start(pi);
                Thread.Sleep(100);    // unexpected exit on linux
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }

                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        private bool ShowWindowChrome2(string path)
        {
            try

            {
                var cmd = Environment.OSVersion.Platform == PlatformID.Unix ? "chromium-browser" : "chrome.exe";
                var parameter = $"--app={_startUrl}/?title={HttpUtility.UrlEncode(_title)} --window-size={_windowSize.X},{_windowSize.Y} --disable-translate --user-data-dir=\"{path}\"";
                var ui = Process.Start(cmd, parameter);
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }
                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }


        private bool ShowWindowEdge(string path)
        {
            try
            {
                var pi = new ProcessStartInfo
                {
                    FileName = "msedge",
                    CreateNoWindow = true,
                    Arguments = $"--app={_startUrl}/?title={HttpUtility.UrlEncode(_title)} --window-size={_windowSize.X},{_windowSize.Y} --disable-translate --user-data-dir=\"{path}\"",
                    UseShellExecute = Environment.OSVersion.Platform != PlatformID.Unix
                };
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    pi.Arguments += " --disable-gpu";
                }
                var ui = Process.Start(pi);
                Thread.Sleep(100);    // unexpected exit on linux
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }

                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }


        private bool ShowWindowEpiphany()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return false;

            try
            {
                var parameter = $"{_startUrl}/?title={HttpUtility.UrlEncode(_title)}";
                var ui = Process.Start("epiphany", parameter);
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }
                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        private bool ShowWindowMidori(string path)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return false;

            try
            {
                var parameter = $"-e Navigationbar -c {path} -a {_startUrl}/?title={HttpUtility.UrlEncode(_title)}";
                var ui = Process.Start("midori", parameter);
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }
                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        private bool ShowWindowInternetExplorer()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return false;

            try
            {
                const string cmd = "iexplore.exe";
                var parameter = $"-private {_startUrl}/?title={HttpUtility.UrlEncode(_title)}";
                var ui = Process.Start(cmd, parameter);
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }
                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        private bool ShowWindowFirefox()
        {
            try
            {
                var cmd = Environment.OSVersion.Platform == PlatformID.Unix ? "firefox" : "firefox.exe";
                var parameter = $"{_startUrl}/?title={HttpUtility.UrlEncode(_title)} -width {_windowSize.X} -height {_windowSize.Y}";
                var ui = Process.Start(cmd, parameter);
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }
                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        private bool ShowWindowSafari()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return false;

            try
            {
                const string cmd = "safari.exe";
                var parameter = $"-url {_startUrl}/?title={HttpUtility.UrlEncode(_title)} -width {_windowSize.X} -height {_windowSize.Y}";
                var ui = Process.Start(cmd, parameter);
                if ((ui == null) || ui.HasExited)
                {
                    return false;
                }
                Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, _startUrl);
                ui.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

    }
}
