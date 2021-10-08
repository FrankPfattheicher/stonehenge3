namespace IctBaden.Stonehenge3.TreeView.ViewModels
{
    public interface IExpandedProvider
    {
        bool GetExpanded(string id);
        void SetExpanded(string id, bool expanded);
    }
}