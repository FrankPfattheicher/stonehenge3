﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.ViewModel;
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Stonehenge3.Resources
{
    public class StonehengeResourceLoader : IStonehengeResourceProvider
    {
        public List<IStonehengeResourceProvider> Providers { get; }
        public readonly ServiceContainer Services;

        public StonehengeResourceLoader(List<IStonehengeResourceProvider> loaders = null)
        {
            Providers = loaders ?? new List<IStonehengeResourceProvider>();
            Services = new ServiceContainer();
        }

        public void InitProvider(StonehengeResourceLoader loader, StonehengeHostOptions options)
        {
            foreach (var provider in Providers)
            {
                provider.InitProvider(loader, options);
            }
        }


        
        public void Dispose()
        {
            Providers.ForEach(l => l.Dispose());
            Providers.Clear();
        }

        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            return Providers.Select(loader => loader.Post(session, resourceName, parameters, formData))
                .FirstOrDefault(resource => resource != null);
        }
        public Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            var disableCache = false;

            if (resourceName.Contains("${") || resourceName.Contains("{{"))
            {
                resourceName = ReplaceFields(session, resourceName);
                disableCache = true;
            }

            Resource loadedResource = null;
            foreach (var loader in Providers)
            {
                try
                {
                    loadedResource = loader.Get(session, resourceName, parameters);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"StonehengeResourceLoader.{loader.GetType().Name}({resourceName}) exception: {ex.Message}" + 
                                     Environment.NewLine + ex.StackTrace);
                }
                if (loadedResource != null) break;
            }

            if (disableCache)
            {
                loadedResource?.SetCacheMode(Resource.Cache.None);
            }

            return loadedResource;
        }
        
        private string ReplaceFields(AppSession session, string resourceName)
        {
            // support es6 format "${}"
            var replaced = string.Empty;
            while (resourceName.Length > 0)
            {
                var start = resourceName.IndexOf("${", StringComparison.InvariantCulture);
                var closing = 1;
                if (start == -1)
                {
                    start = resourceName.IndexOf("{{", StringComparison.InvariantCulture);
                    closing = 2;
                }
                if (start == -1)
                {
                    replaced += resourceName;
                    break;
                }
                replaced += resourceName.Substring(0, start);
                var field = resourceName.Substring(start + 2);
                resourceName = resourceName.Substring(start + 2);

                var end = field.IndexOf('}');
                field = field.Substring(0, end);

                if (session.Cookies.ContainsKey(field))
                {
                    replaced += session.Cookies[field];
                }

                resourceName = resourceName.Substring(end + closing);
            }
            return replaced;
        }

        public static StonehengeResourceLoader CreateDefaultLoader(IStonehengeResourceProvider provider)
        {
            return CreateDefaultLoader(provider, Assembly.GetCallingAssembly());
        }
        public static StonehengeResourceLoader CreateDefaultLoader(IStonehengeResourceProvider provider, Assembly appAssembly)
        {
            var assemblies = new List<Assembly>
                 {
                     appAssembly,
                     Assembly.GetEntryAssembly(),
                     Assembly.GetExecutingAssembly(),
                     Assembly.GetAssembly(typeof(ResourceLoader))
                 }
                .Distinct()
                .ToList();

            var resLoader = new ResourceLoader(assemblies, appAssembly);
            if (provider != null)
            {
                resLoader.AddAssembly(provider.GetType().Assembly);
            }

            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Directory.GetCurrentDirectory();
            var fileLoader = new FileLoader(Path.Combine(path, "app"));

            var viewModelCreator = new ViewModelProvider();

            var loader = new StonehengeResourceLoader(new List<IStonehengeResourceProvider> { fileLoader, resLoader, viewModelCreator });
            if (provider != null)
            {
                loader.Providers.Add(provider);
            }
            return loader;
        }

    }
}
