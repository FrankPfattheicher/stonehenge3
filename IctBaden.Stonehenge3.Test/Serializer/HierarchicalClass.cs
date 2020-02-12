using System.Collections.Generic;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace IctBaden.Stonehenge3.Test.Serializer
{
    public class HierarchicalClass
    {
        public string Name { get; set; }
        public List<HierarchicalClass> Children { get; set; }
    }
}