using System;
using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;

// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Vue.Test.ViewModels
{
    // ReSharper disable once UnusedType.Global
    public class StartVm : ActiveViewModel
    {
        public int VmPropInteger { get; set; }
        public string VmPropText { get; set; }
        public List<string> VmPropList { get; set; }
        public Notify<string> VmPropNotify { get; set; }

        private readonly VueTestData _data;
        
        public StartVm(AppSession session, VueTestData data)
        : base(session)
        {
            _data = data;
            _data.DoAction += OnDoAction;
        }

        private string OnDoAction(string action)
        {
            if (action == "Notify")
            {
                VmPropNotify.Update(Guid.NewGuid().ToString());
            }
            return "";
        }

        public override void OnLoad()
        {
            _data.StartVmParameters = Session.Parameters;
            _data.StartVmOnLoadCalled++;
        }
    }
}
