using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Vue.Test.Client
{
    public class DiComponentVm : ActiveViewModel
    {
        public int VmPropInteger { get; set; }
        public string VmPropText { get; set; }
        public List<string> VmPropList { get; set; }

        private readonly DiDependency _dependency;

        public DiComponentVm(AppSession session, DiDependency dependency)
            : base(session)
        {
            _dependency = dependency;
        }
    }
}