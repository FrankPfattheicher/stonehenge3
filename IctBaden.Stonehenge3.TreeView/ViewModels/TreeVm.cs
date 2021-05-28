using System.Collections.Generic;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
// ReSharper disable MemberCanBeProtected.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace IctBaden.Stonehenge3.TreeView.ViewModels
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedType.Global
    public class TreeVm : ActiveViewModel
    {
        private TreeNodeVm _rootNode;

        public List<TreeNodeVm> RootNodes { get; private set; }
        public TreeNodeVm SelectedNode { get; private set; }

        // ReSharper disable once UnusedMember.Global
        protected TreeVm(AppSession session) : base (session)
        {
            RootNodes = new();
        }

        protected void SetRootNode(object item, bool showRootNode = true, bool expandRootNodes = false)
        {
            _rootNode = new TreeNodeVm(null, item);
            RootNodes = showRootNode
                ? new List<TreeNodeVm> { _rootNode }
                : _rootNode.Children;

            if (!expandRootNodes) return;
            
            foreach (var node in RootNodes)
            {
                node.IsExpanded = true;
            }
        }

        public IEnumerable<TreeNodeVm> AllNodes() => _rootNode.AllNodes();
        
        
        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void TreeToggle(string nodeId)
        {
            var node = _rootNode.FindNodeById(nodeId);
            if (node == null) return;

            node.IsExpanded = !node.IsExpanded;
        }

        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void TreeSelect(string nodeId)
        {
            var node = _rootNode.FindNodeById(nodeId);
            if (node == null) return;

            foreach (var treeNode in _rootNode.AllNodes())
            {
                treeNode.IsSelected = false;
            }
            node.IsSelected = true;
            SelectedNode = node;
            SelectionChanged();
        }

        protected virtual void SelectionChanged()
        {
        }
        

    }
}
