using System;
using System.Reactive.Linq;
using System.Reflection;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.ViewModel;
// ReSharper disable StringLiteralTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    // ReSharper disable once UnusedMember.Global
    public class StartVm : ActiveViewModel, IDisposable
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        [DependsOn(nameof(AutoNotify))]
        public string TimeStamp => DateTime.Now.ToLongTimeString();
        public Notify<string> AutoNotify { get; set; }
        public double Numeric { get; set; }
        public string Test { get; set; }
        public string Version => Assembly.GetEntryAssembly()?.GetName().Version.ToString(2);
        public bool IsLocal => Session?.IsLocal ?? true;

        private IDisposable _updater;
        
        // ReSharper disable once UnusedMember.Global
        public StartVm(AppSession session) : base (session)
        {
            Numeric = 123.456;
            Test = "abcd";

            _updater = Observable
                .Interval(TimeSpan.FromSeconds(2))
                .Subscribe(_ =>
                {
                    AutoNotify.Update(TimeStamp.ToString());
                });
        }

        public void Dispose()
        {
            _updater?.Dispose();
            _updater = null;
        }

        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void Save(int number, string text)
        {
            Test = number + Test + text;
        }

        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void ShowMessageBox()
        {
            MessageBox("Stonehenge 3", $"Server side message box request. {Test}");
        }

        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void NavigateToTree()
        {
            NavigateTo("tree");
        }

        [ActionMethod]
        // ReSharper disable once UnusedMember.Global
        public void NavigateOnPage()
        {
            NavigateTo("#pagetop");
        }

        public override Resource GetDataResource(string resourceName)
        {
            if (resourceName.EndsWith(".ics"))
            {
                var cal = @"BEGIN:VCALENDAR
PRODID:-//ICT Baden GmbH//Framework Library 2016//DE
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
BEGIN:VEVENT
UID:902af1f31c454e5983d707c6d7ee3d4a
DTSTART:20160501T181500Z
DTEND:20160501T194500Z
DTSTAMP:20160501T202905Z
CREATED:20160501T202905Z
LAST-MODIFIED:20160501T202905Z
TRANSP:OPAQUE
STATUS:CONFIRMED
ORGANIZER:ARD
SUMMARY:Tatort
END:VEVENT
END:VCALENDAR
";
                return new Resource(resourceName, "Sample", ResourceType.Calendar, cal, Resource.Cache.None);
            }
            return new Resource(resourceName, "Sample", ResourceType.Text, $"This ist the content of {resourceName} file ;-)", Resource.Cache.None);
        }
    }
}
