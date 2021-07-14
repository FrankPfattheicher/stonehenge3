using System.Linq;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    // ReSharper disable once UnusedType.Global
    public class FormsVm : ActiveViewModel
    {
        public int Range { get; set; }
        public int RangeMin { get; } = 0;
        public int RangeMax { get; } = 40;

        public C3Chart ChartData { get; }

        public bool ShowCookies { get; private set; }
        
        public FormsVm(AppSession session) : base(session)
        {
            Range = 20;

            const string column1 = "Temperature";
            ChartData = new C3Chart(new []{column1});

            ChartData.Data.SetData(0, new object [] {10, 12, 15, 14, 13, 20, 22, 25, Range});

            ChartData.Axis["y"] = new C3ChartAxis { min = 0, max = 40 };
        }

        [ActionMethod]
        public void RangeChanged()
        {
            var newData = ChartData.Data.GetData(0);
            newData = newData.Take(newData.Length - 1)
                .Concat(new object[] { Range })
                .ToArray();
            ChartData.Data.SetData(0, newData);
        }

        [ActionMethod]
        public void ToggleShowCookies()
        {
            ShowCookies = !ShowCookies;
            EnableRoute("cookie", ShowCookies);
        }
        
    }
}