using System;

namespace IctBaden.Stonehenge3.ViewModel
{
    /// <summary>
    /// Replacement for System.Windows.Markup.DependsOnAttribute
    /// for non windows systems
    /// </summary>
    public class DependsOnAttribute : Attribute
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string Name { get; private set; }

        public DependsOnAttribute(string name)
        {
            Name = name;
        }
    }
}
