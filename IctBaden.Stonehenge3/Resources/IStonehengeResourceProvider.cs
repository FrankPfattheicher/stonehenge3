using System;
using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;

namespace IctBaden.Stonehenge3.Resources
{
    public interface IStonehengeResourceProvider : IDisposable
    {
        void InitProvider(StonehengeResourceLoader loader, StonehengeHostOptions options);
        List<ViewModelInfo> GetViewModelInfos();
        
        Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters);

        Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData);
    }
}
