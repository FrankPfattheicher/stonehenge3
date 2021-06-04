using System;
using IctBaden.Stonehenge3.Core;

namespace IctBaden.Stonehenge3.TreeView.ViewModels
{
    public class SessionExpandedProvider : IExpandedProvider
    {
        private readonly AppSession _session;

        public SessionExpandedProvider(AppSession session)
        {
            _session = session;
        }
        public bool GetExpanded(string id) => (bool) (Convert.ChangeType(_session[$"expanded{id}"], typeof(bool)) ?? false);
        public void SetExpanded(string id, bool expanded) => _session[$"expanded{id}"] = expanded;
    }
}