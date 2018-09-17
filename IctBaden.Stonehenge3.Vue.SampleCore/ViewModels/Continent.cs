using System.Collections.Generic;

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    public class Continent
    {
        public string Name { get; set; }
        public int Countries { get; set; }
        /// <summary>
        /// 1000 square km
        /// </summary>
        public int Area { get; set; }
        public bool IsChild { get; set; }

        public List<Continent> Children = new List<Continent>();
    }
}
