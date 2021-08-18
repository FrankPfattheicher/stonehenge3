using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    public class ImagesVm : ActiveViewModel
    {
        public bool IsOn { get; set; }

        public string SwitchImg => IsOn 
            ? "images/switch_on.png" 
            : "images/switch_off.png";

        public string LampImg => IsOn 
            ? "images/lightbulb_on.png" 
            : "images/lightbulb.png";

        public ImagesVm(AppSession session)
            : base(session)
        {
        }

        [ActionMethod]
        public void Switch()
        {
            IsOn = !IsOn;
            
            ExecuteClientScript(IsOn 
                ? "document.getElementById('on').play()" 
                : "document.getElementById('off').play()");
        }
        
    }
}