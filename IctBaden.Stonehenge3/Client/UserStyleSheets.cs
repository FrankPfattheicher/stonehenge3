using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IctBaden.Stonehenge3.Client
{
    public class UserStyleSheets
    {
        private const string InsertPoint = "<!--stonehengeUserStylesheets-->";
        private const string LinkTemplate = "<link href='{0}' rel='stylesheet'>";
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
                      .Select(dir => string.Format(LinkTemplate, dir.Substring(dir.IndexOf(AppPath, StringComparison.InvariantCulture) + 1).Replace('\\', '/')));
                    styleSheets = string.Join(Environment.NewLine, links);
                }

                var ressourceBaseName = userAssembly.GetName().Name + ".";
                var baseNameStyles = ressourceBaseName + "app.styles.";
                var baseNameTheme = ressourceBaseName + "app.themes.";
                var ressourceNames = userAssembly.GetManifestResourceNames();
                var cssRessources = ressourceNames.Where(name => name.EndsWith(".css")).ToList();
                // styles first
                foreach (var resourceName in cssRessources.Where(name => name.StartsWith(baseNameStyles, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var css = resourceName.Substring(ressourceBaseName.Length).Replace(".", "/").Replace("/css", ".css");
                    styleSheets += Environment.NewLine + string.Format(LinkTemplate, css);
                }
                // then themes
                foreach (var resourceName in cssRessources.Where(name => name.StartsWith(baseNameTheme + theme, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var css = resourceName.Substring(ressourceBaseName.Length).Replace(".", "/").Replace("/css", ".css");
                    styleSheets += Environment.NewLine + string.Format(LinkTemplate, css);
                }

                path = Path.Combine(appFilesPath, "app", "themes", theme + ".css");
                if (File.Exists(path))
                {
                    var css = path.Substring(path.IndexOf(AppPath, StringComparison.InvariantCulture) + 1).Replace('\\', '/');
                    styleSheets += Environment.NewLine + string.Format(LinkTemplate, css);
                }

                if (!StyleSheets.ContainsKey(theme))
                {
                    StyleSheets.Add(theme, styleSheets);
                }
            }
            return text.Replace(InsertPoint, StyleSheets[theme]);
        }
    }
}
