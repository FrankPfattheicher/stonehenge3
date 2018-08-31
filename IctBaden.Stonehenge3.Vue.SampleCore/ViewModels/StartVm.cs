using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.ViewModel;

namespace IctBaden.Stonehenge3.Vue.SampleCore.ViewModels
{
    // ReSharper disable once UnusedMember.Global
    public class StartVm : ActiveViewModel, IDisposable
    {
        public string TimeStamp => DateTime.Now.ToLongTimeString();
        public double Numeric { get; set; }
        public string Test { get; set; }
        public string Version => Assembly.GetEntryAssembly().GetName().Version.ToString(2);

        private Task _updater;
        private readonly CancellationTokenSource _cancelUpdate;

        // ReSharper disable once UnusedMember.Global
        public StartVm(AppSession session) : base (session)
        {
            Numeric = 123.456;
            Test = "abcd";

            _cancelUpdate = new CancellationTokenSource();
            _updater = new Task(
                () =>
                    {
                        while ((_updater != null) && !_cancelUpdate.IsCancellationRequested)
                        {
                            Task.Delay(10000, _cancelUpdate.Token);
                            NotifyPropertyChanged(nameof(TimeStamp));
                        }
                        // ReSharper disable once FunctionNeverReturns
                    }, _cancelUpdate.Token);
            _updater.Start();
        }

        public void Dispose()
        {
            _cancelUpdate.Cancel();
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
