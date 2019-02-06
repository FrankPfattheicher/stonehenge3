using System;
using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Xunit;

namespace IctBaden.Stonehenge3.Test.DiContainer
{
    public class ResolveVmDependenciesTest : IDisposable
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public Guid Id;

        private readonly StonehengeResourceLoader _loader;
        private readonly AppSession _session;

        public ResolveVmDependenciesTest()
        {
            Id = Guid.NewGuid();
            _loader = StonehengeResourceLoader.CreateDefaultLoader();
            _loader.Services.AddService(typeof(ResolveVmDependenciesTest), this);
            _session = new AppSession(_loader, new StonehengeHostOptions());
        }

        public void Dispose()
        {
            _loader.Dispose();
        }

        [Fact]
        public void SimpleVmShouldGetReferenceToThisTest()
        {
            Id = Guid.NewGuid();
            _loader.Get(_session, "ViewModel/" + nameof(TestSimpleVmWithDependency), new Dictionary<string, string>());
            var vm = _session.ViewModel as TestSimpleVmWithDependency;
            Assert.NotNull(vm);
            Assert.Equal(Id, vm.Test.Id);
        }

        [Fact]
        public void ActiveVmShouldGetReferenceToThisTest()
        {
            Id = Guid.NewGuid();
            _loader.Get(_session, "ViewModel/" + nameof(TestActiveVmWithDependency), new Dictionary<string, string>());
            var vm = _session.ViewModel as TestActiveVmWithDependency;
            Assert.NotNull(vm);
            Assert.Equal(_session, vm.Session);
            Assert.Equal(Id, vm.Test.Id);
        }

    }
}
