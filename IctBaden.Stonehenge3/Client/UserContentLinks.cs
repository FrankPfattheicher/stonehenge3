using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IctBaden.Stonehenge3.Resources;

namespace IctBaden.Stonehenge3.Client
{
    public static class UserContentLinks
    {
        private const string CssInsertPoint = "<!--stonehengeUserStylesheets-->";
        private const string CssLinkTemplate = "<link href='{0}' rel='stylesheet'>";

        private const string JsInsertPoint = "<!--stonehengeUserScripts-->";
        private const string JsLinkTemplate = "<script src='{0}'></script>";

        private static readonly Dictionary<string, string> StyleSheets = new Dictionary<string, string>();
        private static readonly string AppPath = Path.DirectorySeparatorChar + "app" + Path.DirectorySeparatorChar;

        public static string InsertUserCssLinks(Assembly userAssembly, string appFilesPath, string text, string theme)
        {
            if (!StyleSheets.ContainsKey(theme))
            {
                var styleSheets = string.Empty;

                var path = Path.Combine(appFilesPath, "styles");
                if (Directory.Exists(path))
                {
                    var links = Directory.GetFiles(path, "*.css", SearchOption.AllDirectories)
                      .Select(dir => string.Format(CssLinkTemplate, dir.Substring(dir.IndexOf(AppPath, StringComparison.InvariantCulture) + 1).Replace('\\', '/')));
                    styleSheets = string.Join(Environment.NewLine, links);
                }

                const string ressourceBaseName = ".app.";
                const string baseNameStyles = ressourceBaseName + "styles.";
                const string baseNameTheme = ressourceBaseName + "themes.";
                var ressourceNames = userAssembly.GetManifestResourceNames();
                var cssResources = ressourceNames.Where(name => name.EndsWith(".css")).ToList();
                // styles first
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var resourceName in cssResources.Where(name => name.Contains(baseNameStyles)))
                {
                    var css = ResourceLoader.GetShortResourceName(ressourceBaseName, resourceName).Replace(".", "/").Replace("/css", ".css");
                    styleSheets += Environment.NewLine + string.Format(CssLinkTemplate, css);
                }
                // then themes
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var resourceName in cssResources.Where(name => name.Contains(baseNameTheme + theme)))
                {
                    var css = ResourceLoader.GetShortResourceName(ressourceBaseName, resourceName).Replace(".", "/").Replace("/css", ".css");
                    styleSheets += Environment.NewLine + string.Format(CssLinkTemplate, css);
                }

                path = Path.Combine(appFilesPath, "app", "themes", theme + ".css");
                if (File.Exists(path))
                {
                    var css = path.Substring(path.IndexOf(AppPath, StringComparison.InvariantCulture) + 1).Replace('\\', '/');
                    styleSheets += Environment.NewLine + string.Format(CssLinkTemplate, css);
                }

                if (!StyleSheets.ContainsKey(theme))
                {
                    StyleSheets.Add(theme, styleSheets);
                }
            }
            return text.Replace(CssInsertPoint, StyleSheets[theme]);
        }

        public static string InsertUserJsLinks(Assembly userAssembly, string appFilesPath, string text)
        {
            var scripts = string.Empty;

            var path = Path.Combine(appFilesPath, "scripts");
            if (Directory.Exists(path))
            {
                var links = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories)
                  .Select(dir => string.Format(JsLinkTemplate, dir.Substring(dir.IndexOf(AppPath, StringComparison.InvariantCulture) + 1).Replace('\\', '/')));
                scripts = string.Join(Environment.NewLine, links);
            }

            const string ressourceBaseName = ".app.";
            const string baseNameScripts = ressourceBaseName + "scripts.";
            var ressourceNames = userAssembly.GetManifestResourceNames();
            var jsResources = ressourceNames.Where(name => name.EndsWith(".js")).ToList();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var resourceName in jsResources.Where(name => name.Contains(baseNameScripts)))
            {
                var js = ResourceLoader.GetShortResourceName(ressourceBaseName, resourceName).Replace(".", "/").Replace("/js", ".js");
                scripts += Environment.NewLine + string.Format(JsLinkTemplate, js);
            }

            return text.Replace(JsInsertPoint, scripts);
        }
    }
}
