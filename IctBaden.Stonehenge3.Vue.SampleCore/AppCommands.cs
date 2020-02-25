using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.ViewModel;

namespace IctBaden.Stonehenge3.Vue.SampleCore
{
    // ReSharper disable once UnusedMember.Global
    public class AppCommands : IStonehengeAppCommands
    {
        // ReSharper disable once UnusedMember.Global
        public void FileOpen(AppSession session)
        {
            var vm = session.ViewModel as ActiveViewModel;
            vm?.MessageBox("AppCommand", "FileOpen");
        }
    }
}
