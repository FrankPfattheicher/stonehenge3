using System;
using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;

namespace IctBaden.Stonehenge3.Resources
{
    public interface IStonehengeResourceProvider : IDisposable
    {
        Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters);

        Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData);
    }
}
