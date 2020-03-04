using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.Types;
using IctBaden.Stonehenge3.Vue.Client;
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Stonehenge3.Vue
{
    public class VueResourceProvider : IStonehengeResourceProvider
    {
        private Dictionary<string, Resource> _vueContent;
        private List<Assembly> _assemblies;
        private Assembly _appAssembly;

        public VueResourceProvider()
        {
            _assemblies = new List<Assembly>
            {
                Assembly.GetEntryAssembly(), 
                Assembly.GetAssembly(GetType())
            };
        }
        
        public void InitProvider(StonehengeResourceLoader loader, StonehengeHostOptions options)
        {
            _vueContent = new Dictionary<string, Resource>();

            if (loader?.Providers
                    .FirstOrDefault(p => p.GetType() == typeof(ResourceLoader)) is ResourceLoader resourceLoader)
            {
                _assemblies = resourceLoader.ResourceAssemblies;
                _appAssembly = resourceLoader.AppAssembly;
            }
            
            var appCreator = new VueAppCreator(options.Title, options.StartPage, loader, options, _appAssembly, _vueContent);

            AddFileSystemContent(options.AppFilesPath);
            AddResourceContent();
            appCreator.CreateApplication();
            appCreator.CreateComponents(loader);
        }

        public void Dispose()
        {
            _vueContent.Clear();
        }

        private static readonly Regex ExtractName = new Regex("<!--ViewModel:(\\w+)-->");
        private static readonly Regex ExtractElement = new Regex("<!--CustomElement(:([\\w, ]+))?-->");
        private static readonly Regex ExtractTitle = new Regex("<!--Title:([^:]*)(:(\\d*))?-->");

        private static ViewModelInfo GetViewModelInfo(string route, string pageText)
        {
            route = string.IsNullOrEmpty(route)
                ? ""
                : route.Substring(0, 1).ToUpper() + route.Substring(1);
            var info = new ViewModelInfo(route + "Vm");

            var match = ExtractElement.Match(pageText);
            if (match.Success)
            {
                info.ElementName = NamingConverter.SnakeToPascalCase(route);
                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    info.Bindings = match.Groups[2].Value
                        .Split(',')
                        .Select(b => b.Trim())
                        .ToList();
                }
                info.VmName = null;
                info.SortIndex = 0;
            }
            else
            {
                match = ExtractName.Match(pageText);
                if (match.Success)
                {
                    info.VmName = match.Groups[1].Value;
                }
                match = ExtractTitle.Match(pageText);
                if (match.Success)
                {
                    info.Title = match.Groups[1].Value;
                    info.SortIndex = (string.IsNullOrEmpty(match.Groups[3].Value))
                        ? 0
                        : int.Parse(match.Groups[3].Value);
                }
                else
                {
                    info.Title = route;
                }
            }
            return info;
        }

        private void AddFileSystemContent(string appFilesPath)
        {
            if (Directory.Exists(appFilesPath))
            {
                var appPath = Path.DirectorySeparatorChar + "app" + Path.DirectorySeparatorChar;
                var appFiles = Directory.GetFiles(appFilesPath, "*.html", SearchOption.AllDirectories);

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var appFile in appFiles)
                {
                    var resourceId = appFile.Substring(appFile.IndexOf(appPath, StringComparison.InvariantCulture) + 5)
                        .Replace("@", "_")
                        .Replace("-", "_")
                        .Replace('\\', '/');
                    var route = resourceId.Replace(".html", string.Empty);
                    var pageText = File.ReadAllText(appFile);

                    var resource = new Resource(route, appFile, ResourceType.Html, pageText, Resource.Cache.OneDay) { ViewModel = GetViewModelInfo(route, pageText) };
                    _vueContent.Add(resourceId, resource);
                }
            }
        }

        private void AddResourceContent()
        {
            foreach (var assembly in _assemblies)
            {
                foreach (var resourceName in assembly.GetManifestResourceNames()
                  .Where(name => (name.EndsWith(".html")) && !name.Contains("index.html") && !name.Contains("src.app.html"))
                  .OrderBy(name => name))
                {
                    var resourceId = ResourceLoader.GetShortResourceName(".app.", resourceName)
                        .Replace("@", "_")
                        .Replace("-", "_")
                        .Replace("._0", ".0")
                        .Replace("._1", ".1")
                        .Replace("._2", ".2")
                        .Replace("._3", ".3")
                        .Replace("._4", ".4")
                        .Replace("._5", ".5")
                        .Replace("._6", ".6")
                        .Replace("._7", ".7")
                        .Replace("._8", ".8")
                        .Replace("._9", ".9");
                    if (_vueContent.ContainsKey(resourceId))
                    {
                        Trace.TraceWarning("VueResourceProvider.AddResourceContent: Resource with id {0} already exits", resourceId);
                        continue;
                    }

                    var route = resourceId.Replace(".html", string.Empty);
                    var pageText = "";
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            // ReSharper disable once ConvertToUsingDeclaration
                            using (var reader = new StreamReader(stream))
                            {
                                pageText = reader.ReadToEnd();
                            }
                        }
                    }

                    var resource = new Resource(route, "res://" + resourceName, ResourceType.Html, pageText, Resource.Cache.Revalidate)
                    {
                        ViewModel = GetViewModelInfo(route, pageText)
                    };
                    var key = "src." + resourceId;
                    if (!_vueContent.ContainsKey(key))
                    {
                        _vueContent.Add(key, resource);
                    }
                }
            }
        }





        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            return null;
        }
        public Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            resourceName = resourceName.Replace("/", ".").Replace("@", "_").Replace("-", "_");
            if (_vueContent.ContainsKey(resourceName))
            {
                return _vueContent[resourceName];
            }
            return null;
        }
    }
}
