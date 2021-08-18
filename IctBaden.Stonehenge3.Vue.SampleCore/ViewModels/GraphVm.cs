using System;
using System.Threading;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    // ReSharper disable once UnusedType.Global
    public class GraphVm : ActiveViewModel
    {
        public int RangeMin { get; } = 0;
        public int RangeMax { get; } = 100;

        public C3Chart ChartData { get; }

        private int _speed;
        private Timer _timer;
        private int _start;
        
        public GraphVm(AppSession session) : base(session)
        {
            _speed = 300;
            
            const string column1 = "Sinus";
            ChartData = new C3Chart(new []{column1});

            UpdateGraph(null);

            ChartData.Axis["y"] = new C3ChartAxis { min = 0, max = 100 };
        }

        public override void OnLoad()
        {
            _timer = new Timer(UpdateGraph, this, _speed, _speed);
        }

        private void UpdateGraph(object _)
        {
            var data = new object [50];
            for (var ix = 0; ix < 50; ix++)
            {
                data[ix] = (int)(Math.Sin((ix * 2 + _start) * Math.PI / 36) * 40) + 50;
            }
            _start++;
            
            ChartData.Data.SetData(0, data);
            Session.UpdatePropertyImmediately(nameof(ChartData));
        }


        [ActionMethod]
        public void ToggleSpeed()
        {
            _timer.Dispose();
            _speed = 400 - _speed;
            _timer = new Timer(UpdateGraph, this, _speed, _speed);
        }
        
    }
}