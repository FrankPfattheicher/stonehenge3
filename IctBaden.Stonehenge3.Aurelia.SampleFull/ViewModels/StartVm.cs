using System;
using System.Reflection;
using System.Threading;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.ViewModel;

namespace IctBaden.Stonehenge3.Aurelia.SampleFull.ViewModels
{
    public class StartVm : ActiveViewModel, IDisposable
    {
        public string TimeStamp => DateTime.Now.ToLongTimeString();
        public double Numeric { get; set; }
        public string Test { get; set; }
        public string Version => Assembly.GetEntryAssembly().GetName().Version.ToString(2);

        private Thread _updater;

        public StartVm(AppSession session) : base (session)
        {
            Numeric = 123.456;
            Test = "54321";
            _updater = new Thread(
                () =>
                    {
                        while (_updater != null)
                        {
                            Thread.Sleep(10000);
                            NotifyPropertyChanged(nameof(TimeStamp));
                        }
                        // ReSharper disable once FunctionNeverReturns
                    });
            _updater.Start();
        }

        public void Dispose()
        {
            //_updater.Interrupt();
            _updater = null;
        }

        [ActionMethod]
        public void Save(int number, string text)
        {
            Test = number + Test + text;
        }

        [ActionMethod]
        public void ShowMessageBox()
        {
            MessageBox("Stonehenge 3", $"Server side message box request. {Test}");
        }

        [ActionMethod]
        public void NavigateToTree()
        {
            NavigateTo("tree");
        }

        [ActionMethod]
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
