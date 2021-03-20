using System;
using System.Collections.Generic;
using System.IO;
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
    // ReSharper disable once UnusedType.Global
    public class StartVm : ActiveViewModel, IDisposable
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        [DependsOn(nameof(AutoNotify))]
        public string TimeStamp => DateTime.Now.ToLongTimeString();
        public Notify<string> AutoNotify { get; set; }
        public double Numeric { get; set; }
        public string Test { get; set; }
        public string Version => Assembly.GetEntryAssembly()!.GetName().Version!.ToString(2);
        public bool IsLocal => Session?.IsLocal ?? true;
        public string ClientAddress => Session.ClientAddress ?? "(unknown)";
        public string UserIdentity => Session.UserIdentity ?? "(unknown)";

        public string NotInitialized { get; set; }

        private IDisposable _updater;
        private string _text = "This ist the content of user file ;-) Press Alt+Left to return.";        
        // ReSharper disable once UnusedMember.Global
        public StartVm(AppSession session) : base (session)
        {
            Numeric = 123.456;
            Test = "abcd";

            _updater = Observable
                .Interval(TimeSpan.FromSeconds(2))
                .Subscribe(_ =>
                {
                    AutoNotify.Update(TimeStamp);
                });
        }

        public void Dispose()
        {
            _updater?.Dispose();
            _updater = null;
        }

        public override void OnLoad()
        {
            Test = Session.Parameters.ContainsKey("test")
                ? Session.Parameters["test"]
                : "0-0";
        }

        [ActionMethod]
        public void CopyTest()
        {
            CopyToClipboard(Test);            
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
            if (!resourceName.EndsWith(".ics"))
            {
                return new Resource(resourceName, "Sample", ResourceType.Text, _text, Resource.Cache.None);
            }
            
            const string cal = @"BEGIN:VCALENDAR
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

        public override Resource PostDataResource(string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            var tempFileName = formData["uploadFile"];
            _text = File.ReadAllText(tempFileName);
            File.Delete(tempFileName);
            return Resource.NoContent;
        }

        [ActionMethod]
        public void ExecJavaScript()
        {
            var js = "var dateSpan = document.createElement('span');"
                     + $"dateSpan.innerHTML = '{DateTime.Now:U}';"
                     + "var insert = document.getElementById('insertion-point');"
                     + "insert.appendChild(dateSpan);"
                     + "insert.appendChild(document.createElement('br'));";
            ExecuteClientScript(js);
        }
    }
}
