using System.Collections.Generic;
using System.Linq;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedType.Global
    public class TreeVm : ActiveViewModel
    {
        private readonly TreeNode _world;
        public List<TreeNode> RootNodes => new List<TreeNode>() { _world };

        public readonly List<Continent> Continents;
        public readonly int TotalArea;
        public readonly int TotalCountries;

        public string SelectedContinent { get; private set; }
        public GaugeData Area { get; private set; }
        public GaugeData Countries { get; private set; }

        // ReSharper disable once UnusedMember.Global
        public TreeVm(AppSession session) : base (session)
        {
            Continents = new List<Continent>
            {
                new Continent { Name = "Asia", Area = 44579, Countries = 50, IsChild = true },
                new Continent { Name = "Africa", Area = 30370, Countries = 54 },
                new Continent { Name = "North America", Area = 24709, Countries = 23, IsChild = true },
                new Continent { Name = "South America", Area = 17840, Countries = 12, IsChild = true },
                new Continent { Name = "Antarctica", Area = 14000, Countries = 0 },
                new Continent { Name = "Europe", Area = 10180, Countries = 51, IsChild = true },
                new Continent { Name = "Australia", Area = 8600, Countries = 14 }
            };

            TotalArea = Continents.Select(c => c.Area).Sum();
            TotalCountries = Continents.Select(c => c.Countries).Sum();

            Continents.Add(new Continent
            {
                Name = "America",
                Area = Continents.Where(c => c.Name.Contains("America")).Select(c => c.Area).Sum(),
                Countries = Continents.Where(c => c.Name.Contains("America")).Select(c => c.Countries).Sum(),
                Children = Continents.Where(c => c.Name.Contains("America")).ToList()
            });
            Continents.Add(new Continent
            {
                Name = "Eurasia",
                Area = Continents.Where(c => c.Name.Contains("Eur") || c.Name.Contains("sia")).Select(c => c.Area).Sum(),
                Countries = Continents.Where(c => c.Name.Contains("Eur") || c.Name.Contains("sia")).Select(c => c.Countries).Sum(),
                Children = Continents.Where(c => c.Name.Contains("Eur") || c.Name.Contains("sia")).ToList()
            });

            _world = new TreeNode(null, null)
            {
                Name = "World",
                IsExpanded = true
            };

            foreach (var continent in Continents.Where(c => !c.IsChild))
            {
                _world.Children.Add(CreateTreeNode(_world, continent));
            }

            Area = new GaugeData
            {
                Name = "Area",
                Value = 0,
                MaxValue = TotalArea,
                Units = "[1000 km²]"
            };
            Countries = new GaugeData
            {
                Name = "Countries",
                Value = 0,
                MaxValue = TotalCountries,
                Units = ""
            };

        }

    private TreeNode CreateTreeNode(TreeNode parent, Continent continent)
        {
            var node = new TreeNode(parent, continent) { Name = continent.Name };
            node.Children = continent.Children
                .Select(c => new TreeNode(node, c) {Name = c.Name})
                .ToList();
            return node;
        }

        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void TreeToggle(string nodeId)
        {
            var node = RootNodes[0].FindNodeById(nodeId);
            if (node == null) return;

            node.IsExpanded = !node.IsExpanded;
        }

        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void TreeSelect(string nodeId)
        {
            var node = _world.FindNodeById(nodeId);
            if (node == null) return;

            foreach (var treeNode in _world.AllNodes())
            {
                treeNode.IsSelected = false;
            }
            node.IsSelected = true;
            SelectedContinent = node.Name;

            if (node.Continent != null)
            {
                Area.Value = node.Continent.Area;
                Countries.Value = node.Continent.Countries;
            }
            else
            {
                Area.Value = TotalArea;
                Countries.Value = TotalCountries;
            }

            NotifyPropertiesChanged(new []
            {
                nameof(SelectedContinent),
                nameof(Area),
                nameof(Countries)
            });
        }

    }
}
