using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IctBaden.Stonehenge3.Client
{
    public class UserContentLinks
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

                var ressourceBaseName = userAssembly.GetName().Name + ".app.";
                var baseNameStyles = ressourceBaseName + "styles.";
                var baseNameTheme = ressourceBaseName + "themes.";
                var ressourceNames = userAssembly.GetManifestResourceNames();
                var cssRessources = ressourceNames.Where(name => name.EndsWith(".css")).ToList();
                // styles first
                foreach (var resourceName in cssRessources.Where(name => name.StartsWith(baseNameStyles, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var css = resourceName.Substring(ressourceBaseName.Length).Replace(".", "/").Replace("/css", ".css");
                    styleSheets += Environment.NewLine + string.Format(CssLinkTemplate, css);
                }
                // then themes
                foreach (var resourceName in cssRessources.Where(name => name.StartsWith(baseNameTheme + theme, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var css = resourceName.Substring(ressourceBaseName.Length).Replace(".", "/").Replace("/css", ".css");
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

            var ressourceBaseName = userAssembly.GetName().Name + ".app.";
            var baseNameScripts = ressourceBaseName + "scripts.";
            var ressourceNames = userAssembly.GetManifestResourceNames();
            var jsRessources = ressourceNames.Where(name => name.EndsWith(".js")).ToList();
            foreach (var resourceName in jsRessources.Where(name => name.StartsWith(baseNameScripts, StringComparison.InvariantCultureIgnoreCase)))
            {
                var js = resourceName.Substring(ressourceBaseName.Length).Replace(".", "/").Replace("/js", ".js");
                scripts += Environment.NewLine + string.Format(JsLinkTemplate, js);
            }

            return text.Replace(JsInsertPoint, scripts);
        }
    }
}
