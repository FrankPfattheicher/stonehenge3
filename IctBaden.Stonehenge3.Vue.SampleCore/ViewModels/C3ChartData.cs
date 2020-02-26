using System.Collections.Generic;
using System.Linq;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    public class C3ChartAxis
    {
        public bool show { get; set; }
        public int min { get; set; }
        public int max { get; set; }

        public C3ChartAxis()
        {
            show = true;
            min = 0;
            max = 100;
        }
    }

    public class C3ChartData
    {
        public object[][] columns { get; private set; }
        
        /// <summary>
        /// Use column name as key, axis id as object.
        /// By default all columns are mapped to axis 'y'
        /// </summary>
        public Dictionary<string, object> axes { get; private set; }
        
        
        public C3ChartData(string[] columnNames)
        {
            columns = columnNames
                .Select(name => new object[]{ name })
                .ToArray();
            axes = new Dictionary<string, object>();
            foreach (var columnName in columnNames)
            {
                axes[columnName] = "y";
            }
        }
        
        public object[] GetData(int columnIndex)
        {
            if (columnIndex >= columns.Length) return new object[0];

            return columns[columnIndex].Skip(1).ToArray();
        }

        public void SetData(int columnIndex, object[] data)
        {
            if (columnIndex >= columns.Length) return;

            columns[columnIndex] = columns[columnIndex].Take(1)
                .Concat(data)
                .ToArray();
        }

        public void AddColumnAxe(string columnName, string axis)
        {
            axes = new Dictionary<string, object>(axes
                .Concat(new[] {new KeyValuePair<string, object>(columnName, axis)})
                .ToArray());
        }
    }
    
    public class C3Chart
    {
        public C3ChartData Data { get; set; }
        
        /// <summary>
        /// Use C3ChartAxis objects.
        /// </summary>
        public Dictionary<string, object> Axis { get; private set; }

        public C3Chart(string[] columnNames)
        {
            Data = new C3ChartData(columnNames);
            Axis = new Dictionary<string, object>();
        }
    }
    
    
}
