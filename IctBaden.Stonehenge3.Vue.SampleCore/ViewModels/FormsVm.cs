using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
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

        public int[] ChartData  { get; private set; } = new[] {1, 2, 3, 2, 2, 4, 5, 5, 3, 4}; 

        public FormsVm(AppSession session) : base(session)
        {
            Range = 33;
        }

        [ActionMethod]
        public void RangeChanged()
        {
            ChartData[^1] = Range / 10;
        }
        
    }
}