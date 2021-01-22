using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
// ReSharper disable StringLiteralTypo

namespace IctBaden.Stonehenge3.Hosting
{
    public static class StonehengeApplication
    {
        /// <summary>
        /// With netcore 3 introduced single file publish,
        /// the assemblies are unpacked in a temp directory.
        /// Assembly.Location returns this temp path :-(
        /// This is a first try to ALWAYS return the applications directory.
        /// Feedback welcome !!!
        /// </summary>
        /// <returns></returns>
        public static string BaseDirectory
        {
            get
            {
                var path = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
                if (IsRunningInUnitTest)
                {
                    return path;
                }

                using var processModule = Process.GetCurrentProcess().MainModule;
                if (processModule != null)
                {
                    var moduleName = processModule.ModuleName.ToLower();
                    if (moduleName != "dotnet" && moduleName != "dotnet.exe")
                    {
                        // started as app.exe (netcore 3.1) or published single file
                        path = Path.GetDirectoryName(processModule.FileName);
                        if (Directory.Exists(path))
                        {
                            return path;
                        }
                    }
                }

                // started as "dotnet <assembly>"
                var assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    path = Path.GetDirectoryName(assembly.Location);
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }

                return Directory.Exists(path) 
                    ? path 
                    : Environment.CurrentDirectory;    // fallback
            }
        }

        private static bool IsRunningInUnitTest
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null) return false;
                
                var moduleName = assembly.GetName().Name!.ToLower();
                return moduleName.Contains("testrunner") || moduleName.Contains("testhost");
            }
        }
        
    }
}