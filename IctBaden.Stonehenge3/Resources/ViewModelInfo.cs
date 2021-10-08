using System.Collections.Generic;

namespace IctBaden.Stonehenge3.Resources
{
    public class ViewModelInfo
    {
        // CustomComponent
        public string ElementName { get; set; }
        public List<string> Bindings { get; set; }

        // ViewModel
        public string VmName { get; set; }
        public string Title { get; set; }
        public int SortIndex { get; set; }
        public bool Visible { get; set; }

        public ViewModelInfo(string name)
        {
            VmName = name;
            SortIndex = 1;    // ensure visible
        }


    }
}