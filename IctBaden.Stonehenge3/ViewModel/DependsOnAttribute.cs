using System;

namespace IctBaden.Stonehenge3.ViewModel
{
    public class DependsOnAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
