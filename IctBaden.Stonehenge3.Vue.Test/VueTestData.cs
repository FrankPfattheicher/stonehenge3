using System;
using System.Collections.Generic;

namespace IctBaden.Stonehenge3.Vue.Test
{
    public class VueTestData
    {
        public Dictionary<string, string> StartVmParameters { get; set; }
        public int StartVmOnLoadCalled { get; set; }
        
        public event Func<string, string> DoAction;

        public string ExecAction(string action) => DoAction?.Invoke(action) ?? null;
    }
}