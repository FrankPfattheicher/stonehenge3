using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Xunit;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace IctBaden.Stonehenge3.Vue.Test.Client
{
    public class ControllerCreationTests
    {
        private readonly StonehengeHostOptions _options;
        private readonly AppSession _session;
        private readonly VueResourceProvider _vue;
        private readonly StonehengeResourceLoader _loader;

        
        public ControllerCreationTests()
        {
            _options = new StonehengeHostOptions();
            _vue = new VueResourceProvider(StonehengeLogger.DefaultLogger);
            _loader = StonehengeResourceLoader.CreateDefaultLoader(StonehengeLogger.DefaultLogger, _vue);
            _loader.InitProvider(_loader, _options);
            _loader.Services.AddService(typeof(DiDependency), new DiDependency());
            _session = new AppSession(_loader, _options);
        }

        [Fact]
        public void ProviderShouldGenerateStartComponent()
        {
            var resource = _loader.Get(_session, "start.js", new Dictionary<string, string>());
            Assert.NotNull(resource);
        }
        
        [Fact]
        public void StartComponentShouldHaveExpectedMembers()
        {
            var resource = _loader.Get(_session, "start.js", new Dictionary<string, string>());
            Assert.Contains("VmPropInteger", resource.Text);
            Assert.Contains("VmPropText", resource.Text);
            Assert.Contains("VmPropList", resource.Text);
            
            Assert.DoesNotContain("//stonehengeProperties", resource.Text);
        }

        [Fact]
        public void ProviderShouldGenerateDiComponent()
        {
            var resource = _loader.Get(_session, "dicomponent.js", new Dictionary<string, string>());
            Assert.NotNull(resource);
        }
        
        [Fact]
        public void DiComponentShouldHaveExpectedMembers()
        {
            var resource = _loader.Get(_session, "dicomponent.js", new Dictionary<string, string>());
            Assert.Contains("VmPropInteger", resource.Text);
            Assert.Contains("VmPropText", resource.Text);
            Assert.Contains("VmPropList", resource.Text);
            
            Assert.DoesNotContain("//stonehengeProperties", resource.Text);
        }

    }
}