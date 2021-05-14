// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using IctBaden.Stonehenge3.Client;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using Microsoft.Extensions.Logging;

namespace IctBaden.Stonehenge3.Resources
{
    public class ResourceLoader : IStonehengeResourceProvider
    {
        public readonly List<Assembly> ResourceAssemblies;
        private readonly ILogger _logger;

        /// <summary>
        /// Assembly containing the embedded application resources
        /// in the app folder.
        /// </summary>
        public readonly Assembly AppAssembly;
        private readonly Lazy<Dictionary<string, AssemblyResource>> _resources;

        public void Dispose()
        {
            ResourceAssemblies.Clear();
        }

        public ResourceLoader(ILogger logger, IEnumerable<Assembly> assembliesToUse, Assembly appAssembly)
        {
            ResourceAssemblies = assembliesToUse.ToList();
            _logger = logger;
            AppAssembly = appAssembly;

            _resources = new Lazy<Dictionary<string, AssemblyResource>>(
                () =>
                {
                    var dict = new Dictionary<string, AssemblyResource>();
                    foreach (var assembly in ResourceAssemblies.Where(a => a != null).Distinct())
                    {
                        AddAssemblyResources(assembly, dict);
                    }
                    return dict;
                });
        }

        public void InitProvider(StonehengeResourceLoader loader, StonehengeHostOptions options)
        {
        }

        public static string GetShortResourceName(string baseName, string resourceName)
        {
            var ixBase = resourceName.IndexOf(baseName, StringComparison.InvariantCultureIgnoreCase);
            return ixBase >= 0 
                ? resourceName.Substring(ixBase + baseName.Length) 
                : string.Empty;
        }

        private static readonly Regex RscName = new Regex(@"\w+://(.*)", RegexOptions.Compiled | RegexOptions.Singleline);  
        public static string RemoveResourceProtocol(string resourceName)
        {
            var match = RscName.Match(resourceName);
            return match.Success 
                ? match.Groups[1].Value 
                : resourceName;
        }

        public void AddAssembly(Assembly assembly)
        {
            ResourceAssemblies.Add(assembly);

            var asmResources = _resources.Value;
            if (asmResources.Values.Any(res => res.Assembly == assembly)) 
                return;

            AddAssemblyResources(assembly, asmResources);
        }

        private void AddAssemblyResources(Assembly assembly, IDictionary<string, AssemblyResource> dict)
        {
            foreach (var resource in assembly.GetManifestResourceNames())
            {
                var shortName = GetShortResourceName(".app.", resource);
                if (string.IsNullOrEmpty(shortName))
                {
                    continue;
                }
                var resourceId = shortName
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
                var asmResource = new AssemblyResource(resource, shortName, assembly);
                if (!dict.ContainsKey(resourceId))
                {
                    _logger.LogDebug($"ResourceLoader.AddAssemblyResources: Added {shortName}");
                    dict.Add(resourceId, asmResource);
                }
            }
        }

        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            return null;
        }

        private string GetAssemblyResourceName(string name) => name
            .Replace("@", "_")
            .Replace("-", "_")
            .Replace("/", ".");
        
        
        public Resource Get(AppSession session, string name, Dictionary<string, string> parameters)
        {
            var resourceName = GetAssemblyResourceName(name);

            var asmResource = _resources.Value
                .FirstOrDefault(res => string.Compare(res.Key, resourceName, true, CultureInfo.InvariantCulture) == 0);
            if (asmResource.Key == null)
            {
                _logger.LogInformation($"ResourceLoader({resourceName}): not found");
                return null;
            }

            var resourceExtension = Path.GetExtension(resourceName);
            var resourceType = ResourceType.GetByExtension(resourceExtension);
            if (resourceType == null)
            {
                _logger.LogInformation($"ResourceLoader({resourceName}): not found");
                return null;
            }

            using (var stream = asmResource.Value.Assembly.GetManifestResourceStream(asmResource.Value.FullName))
            {
                if (stream != null)
                {
                    if (resourceType.IsBinary)
                    {
                        // ReSharper disable once ConvertToUsingDeclaration
                        using (var reader = new BinaryReader(stream))
                        {
                            var data = reader.ReadBytes((int)stream.Length);
                            _logger.LogDebug($"ResourceLoader({resourceName}): {asmResource.Value.FullName}");
                            return new Resource(resourceName, "res://" + asmResource.Value.FullName, resourceType, data, Resource.Cache.Revalidate);
                        }
                    }
                    else
                    {
                        // ReSharper disable once ConvertToUsingDeclaration
                        using (var reader = new StreamReader(stream))
                        {
                            var text = reader.ReadToEnd();
                            _logger.LogDebug($"ResourceLoader({resourceName}): {asmResource.Value.FullName}");
                            text = text.Replace("{.min}", (session?.IsDebug ?? false) ? "" : ".min");
                            if (resourceName?.EndsWith("index.html", StringComparison.InvariantCultureIgnoreCase) ?? false)
                            {
                                text = UserContentLinks.InsertUserCssLinks(AppAssembly, "", text, session?.SubDomain ?? "");
                                text = UserContentLinks.InsertUserJsLinks(AppAssembly, "", text);
                            }
                            return new Resource(resourceName, "res://" + asmResource.Value.FullName, resourceType, text, Resource.Cache.Revalidate);
                        }
                    }
                }
            }

            _logger.LogInformation($"ResourceLoader({resourceName}): not found");
            return null;
        }
    }
}
