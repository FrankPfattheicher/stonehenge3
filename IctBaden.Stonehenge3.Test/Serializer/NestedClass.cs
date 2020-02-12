using System.Collections.Generic;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Test.Serializer
{
    public class NestedClass
    {
        public string Name { get; set; }
        public List<NestedClass2> Nested { get; set; }
    }

    public class NestedClass2
    {
        public SimpleClass[] NestedSimple { get; set; }
    }
}