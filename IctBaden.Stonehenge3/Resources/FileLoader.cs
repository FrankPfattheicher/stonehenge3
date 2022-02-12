using System.Collections.Generic;
using System.IO;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using Microsoft.Extensions.Logging;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Stonehenge3.Resources
{
    public class FileLoader : IStonehengeResourceProvider
    {
        private readonly ILogger _logger;
        public string RootPath { get; private set; }

        public FileLoader(ILogger logger, string path)
        {
            _logger = logger;
            RootPath = path;
        }
        
        public void InitProvider(StonehengeResourceLoader loader, StonehengeHostOptions options)
        {
        }

        public List<ViewModelInfo> GetViewModelInfos() => new List<ViewModelInfo>();

        public void Dispose()
        {
        }

        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            return null;
        }
        public Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            var fullFileName = Path.Combine(RootPath, resourceName);
            if(!File.Exists(fullFileName)) return null;

            var resourceExtension = Path.GetExtension(resourceName);
            var resourceType = ResourceType.GetByExtension(resourceExtension);
            if (resourceType == null)
            {
                _logger.LogInformation($"FileLoader({resourceName}): not found");
                return null;
            }

            _logger.LogTrace($"FileLoader({resourceName}): {fullFileName}");
            if (resourceType.IsBinary)
            {
                return new Resource(resourceName, "file://" + fullFileName, resourceType, File.ReadAllBytes(fullFileName), Resource.Cache.OneDay);
            }

            return new Resource(resourceName, "file://" + fullFileName, resourceType, File.ReadAllText(fullFileName), Resource.Cache.OneDay);
        }

    }
}
