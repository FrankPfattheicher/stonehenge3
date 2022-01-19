using System.Linq;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.ViewModel;
using Microsoft.Extensions.Logging;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace IctBaden.Stonehenge3.Vue.SampleCore
{
    // ReSharper disable once UnusedMember.Global
    public class AppCommands : IStonehengeAppCommands
    {
        private readonly ILogger _logger;

        public AppCommands(ILogger logger)
        {
            _logger = logger;
        }
        
        
        // ReSharper disable once UnusedMember.Global
        public void FileOpen(AppSession session)
        {
            var vm = session.ViewModel as ActiveViewModel;
            vm?.MessageBox("AppCommand", "FileOpen");
        }

        public void WindowResized(AppSession session, int width, int height)
        {
            var paramWidth = session.Parameters.FirstOrDefault(p => p.Key == "width").Value;
            var paramHeight = session.Parameters.FirstOrDefault(p => p.Key == "height").Value;
            
            _logger.LogTrace($"AppCommands.WindowResized: width={width}, height={height}");
        }
        
    }
}
