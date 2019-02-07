using IctBaden.Stonehenge3.Hosting;

namespace IctBaden.Stonehenge3.Test.Tools
{
    using System.Collections.Generic;
    using System.IO;

    using Core;
    using Stonehenge3.Resources;

    public class TestResourceLoader : IStonehengeResourceProvider
    {
        private readonly string _content;

        public TestResourceLoader(string content)
        {
            _content = content;
        }

        public void InitProvider(StonehengeHostOptions options)
        {
        }

        public void Dispose()
        {
        }

        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            return null;
        }

        public Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            var resourceExtension = Path.GetExtension(resourceName);
            return new Resource(resourceName, "test://TestResourceLoader.content", ResourceType.GetByExtension(resourceExtension), _content, Resource.Cache.None);
        }
    }
}
