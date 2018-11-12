using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;

namespace IctBaden.Stonehenge3.Test.DiContainer
{
    public class TestActiveVmWithDependency : ActiveViewModel
    {
        public readonly ResolveVmDependenciesTest Test;

        public TestActiveVmWithDependency(AppSession session, ResolveVmDependenciesTest test)
            : base(session)
        {
            Test = test;
        }

    }
}
