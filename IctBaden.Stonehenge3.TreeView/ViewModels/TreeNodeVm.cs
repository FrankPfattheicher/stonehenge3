using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.TreeView.ViewModels
{
    public class TreeNodeVm
    {
        public string Id { get; }
        public string Icon { get; private set; } // fa fa-folder, fa fa-folder-open
        public string Name { get; set; }

        public List<TreeNodeVm> Children { get; set; }

        // ReSharper disable once UnusedMember.Global
        public bool IsVisible => Parent?.IsExpanded ?? true;
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }

        // ReSharper disable once UnusedMember.Global
        public bool HasChildren => Children.Count > 0;
        public bool IsDraggable { get; internal set; }

        
        public string Class => IsSelected ? "tree-selected" : "";

        public string ExpandIcon => HasChildren
            ? (IsExpanded ? "fa fa-caret-down" : "fa fa-caret-right")
            : "fa";

        
        private readonly IExpandedProvider _expanded;
        public readonly TreeNodeVm Parent;
        public readonly object Item;
        
        public TreeNodeVm(TreeNodeVm parentNodeVm, object item, IExpandedProvider expanded = null)
        {
            Item = item;
            Id = (GetItemProperty("Id") as string) ?? Guid.NewGuid().ToString("N");
            _expanded = expanded;
            Parent = parentNodeVm;
            Children = new List<TreeNodeVm>();
            
            IsExpanded = _expanded?.GetExpanded(Id) ?? false;
            IsDraggable = parentNodeVm != null;

            Name = GetItemProperty("Name") as string;
            Icon = GetItemProperty("Icon") as string;
            
            CreateChildCfgNodes();
        }

        private object GetItemProperty(string propertyName)
        {
            if (Item == null) return null;
            var prop = Item.GetType().GetProperty(propertyName);
            return prop == null ? null : prop.GetValue(Item);
        }
        
        private void CreateChildCfgNodes()
        {
            if (!(GetItemProperty("Children") is IEnumerable<object> children)) return;
            
            if (Children.Any()) return;
            
            foreach (var child in children)
            {
                var childNode = new TreeNodeVm(this, child, _expanded);
                childNode.CreateChildCfgNodes();
                Children.Add(childNode);
            }
        }
        
        public IEnumerable<TreeNodeVm> AllNodes()
        {
            yield return this;
            foreach (var node in Children.SelectMany(child => child.AllNodes()))
            {
                yield return node;
            }
        }

        public TreeNodeVm FindNodeById(string id)
        {
            return AllNodes().FirstOrDefault(node => node.Id == id);
        }
    }
}