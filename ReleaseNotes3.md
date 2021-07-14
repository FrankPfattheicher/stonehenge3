
* 3.19.03 - Fixed automatic start page using negative indexes.
* 3.19.02 - Initial disabled pages using negative index. EnableRoute method in IStonehengeHost.
* 3.19.01 - Start page name convention fixed. Log improved. 
  Initial disabled pages option. EnableRoute method in ActiveViewModel. 
* 3.18.05 - Fixed CopyToClipboard escaping
* 3.18.04 - StonehengeHostOptions.DisableClientCache
* 3.18.03 - Cleanup NotifyAllProperty changes. Delay concurrent client posts in client. 
* 3.18.02 - NotifyAllPropertiesChanged after handling POST.
* 3.18.01 - Do NOT cancel POST and VM requests, event queries only. Removed lang from index html.
* 3.17.12 - Added request header 'Stonehenge-Id'.
* 3.17.11 - Added v-select custom directive.
* 3.17.10 - Added v-focus custom directive to replace no longer working autofocus.
* 3.17.9 - Fixed CopyToClipboard with text containing new lines.
* 3.17.8 - Fixed setting Notify<T>. Client event handling improved. Abort ALL pending client requests.
* 3.17.7 - CopyToClipboard
* 3.17.6 - Support in-proc and out-of-proc IIS hosting
* 3.17.5 - IISIntegration added again
* 3.17.4 - Removed unused Configure parameter in Startup leading to unnecessary dependency
* 3.17.3 - Removed dependency  Microsoft.AspNetCore.Http.Abstractions, App has to use Microsoft.NET.Sdk.Web
* 3.17.2 - Removed dependency Microsoft.AspNetCore.Authentication
* 3.17.1 - Moved from System.Trace to ILogger. PollIntervalMs changed to PollIntervalSec.
* 3.16.8 - Introduced StonehengeApplication to determine base directory also for single-file published apps.
* 3.16.7 - Moved from "Microsoft.AspNetCore.Http" to "Microsoft.Extensions.Http".
* 3.16.6 - Moved Windows specific code to static class WindowsHosting.
* 3.16.5 - Moved from "Microsoft.AspNetCore.Hosting.Abstractions" to "Microsoft.Extensions.Hosting.Abstractions".
* 3.16.4 - UseIIS on Windows only.
* 3.16.3 - Fixed nuspec (netcoreapp3.1).
* 3.16.2 - Removed UseSSL option. Using SSL if cert path is given and exists.
* 3.16.1 - Moved to netcoreapp3.1 and C# 8. Removed synchronous writes. Supporting IIS hosting.
* 3.15.2 - Fixed missing HttpMultipartParser nuget package reference.
* 3.15.1 - Added hosting option UseBasicAuth. Support POST Data/ handling for file upload.
* 3.14.2 - Redirect not existing index.html with path to root document.
* 3.14.1 - Get UserIdentity from authentication header (Basic or Bearer subject).
* 3.13.2 - Fixed nuget package references.
* 3.13.1 - Replacements removed from resource loader. Local fontawesome on iPhone fixed by renaming to fa_free.
* 3.12.2 - Added missing package references in nuget package.
* 3.12.1 - Support NTLM authentication (HttpSys, Windows only) and SSL.
* 3.11.9 - Version number in all DLLs
* 3.11.8 - Assembly version - next try
* 3.11.7 - Assembly version !?
* 3.11.6 - Packages updated. Build with assembly version fixed.
* 3.11.5 - URL session id prioritized. Handling multiple cookie session ids fixed.
* 3.11.4 - Fixed absolute paths for new session redirects.
* 3.11.3 - Use relative paths for client action method posts.
* 3.11.2 - Fixed content invoke lock with events collision.
* 3.11.1 - Use relative paths for redirection and events. Ignore double // at root.
* 3.10.18 - app.js deployed as JS instead of HTML.
* 3.10.17 - Added Cors to Kestrel.
* 3.10.16 - Using $(Version) in csproj files.
* 3.10.15 - Updated version to publish new package.
* 3.10.14 - Custom elements in external files supported.
* 3.10.13 - C3 gauge and chart named as 'c3'. C3Chart class.
* 3.10.13 - Update to Vue.js v2.6.11, Bootstrap v4.4.1. Simple chart added to forms page.
* 3.10.12 - Nuspec file name case and package reference versions fixed.
* 3.10.11 - AppCommand handling fixed. Sample forms page. Create component uses DI container.
* 3.10.10 - HostWindow usable without StonehengeHost. Host's BaseUrl as browsable address.
* 3.10.9 - NavigateTo from background task fixed.
* 3.10.8 - Nuspec file package references fixed.
* 3.10.7 - Enabled NavigateTo from background task.
* 3.10.6 - Serialize Dictionary<string, object> as Json objects.
* 3.10.5 - NotifyAllPropertiesChanged, ActionMethodAttribute.ExecuteAsync
* 3.10.4 - Fixed Title comment without explicit sort index now results in zero (invisible). 
* 3.10.3 - Renamed OnNavigate to OnLoad. Tests added. Test WebClient replaces by HttpClient. 
* 3.10.2 - Added Parameters collection in application session to access URL parameters.
* 3.10.1 - Added tests for required embedded vue resources. OnNavigate virtual VM method.
* 3.9.6 - Added missing embedded resource fontawesome_all.min.css
* 3.9.5 - Use AppAssembly for search of custom components.
* 3.9.4 - Debug.Assert(false) removed, ViewModelProvider exception handler improved.
* 3.9.3 - Removed obsolete IndexPage option.
* 3.9.2 - Fixed handling of multiple application using appAssembly.
* 3.9.1 - Added hosting option IndexPage to explicit specify index template.
* 3.8.10 - Font Awesome woff files updated also
* 3.8.9 - Updated to Font Awesome Free 5.11.2
* 3.8.8 - Added some error handling on missing content.
* 3.8.7 - Fixed Session.ClientAddress.
* 3.8.6 - Use first page as default start page.
* 3.8.5 - Use '' for initialization to prevent '{}' display.
* 3.8.4 - Fixed initial binding errors due to initialize with null, now {}.
* 3.8.3 - Fixed custom elements with multiple bindings code generation.
* 3.8.2 - Fixed Vue warning missing key in routes. Fixed used assemblies with kestrel host.
* 3.8.1 - Fixed Vue component creation from ViewModel with dependency injection.
* 3.8.0 - Updated to Vue 2.6.10, Bootstrap-vuw 2.0.0, Bootstrap 4.3.1, {.min} support added.
* 3.7.0 - Removed Aurelia support.
* 3.6.0 - Added GZIP compression and ACME support.
* 3.5.0 - Added Notify<T> base class to support INotifyPropertyChange for VM properties.
* 3.4.0 - Added app-commands handling using POST /Command/Name. IStonehengeAppCommands.
* 3.3.12 - Do not create ViewModel in VueAppCreator more than once, ensure destroyed.
* 3.3.11 - Added missing ctor to DependsOnAttribute.
* 3.3.10 - Updated nuget to add references.
* 3.3.9 - Fixed Session.IsLocal.
* 3.3.8 - Added Network.GetFreeTcpPort to enable 0 as port to detect any free.
* 3.3.7 - Fixed user content links. Tests added.
* 3.3.6 - Removed title and ssl parameters from host Start(). Using options instead.
* 3.3.5 - Fixed different assembly/namespace problems. Title and start page added to options to hide InitProvider.
* 3.3.4 - ServerPushModes ShortPolling and LongPolling implemented.
* 3.3.3 - Using HostOptions everywhere. 
* 3.3.2 - SimpleHttp session handling fixed. 
* 3.3.1 - Introduced StonehengeHostOptions. Kestrel on linux hanging fixed. 
          Linux HostWindow fixed. SimpleHttp query parameter handling added. 
* 3.2.1 - Removed integrity specification for fontawesome_all.css.
* 3.2.0 - All necessary resources embedded for offline functionality.
* 3.1.1 - Removed vulnerability dependencies from aurelia packages.
* 3.1.0 - Services DI container added to StonehengeResourceLoader.
* 3.0.26 - Icon URL in nuget package fixed.
* 3.0.25 - Abort pending event requests on next request. Packages updated.
* 3.0.24 - Do not handle event results if post is active.
* 3.0.23 - Fixed app reload after server restart.
* 3.0.22 - Fixed app title. Added navigation to root page on startup.
* 3.0.21 - Microsoft packages updated. https deprecated.
* 3.0.20 - Request for events does not set VM and returns StonehengeContinuePolling:false for non active VMs.
* 3.0.19 - Vue added element method hooks. Sample using D3 gauges. Automatic adding links to user scripts in scripts folder.
* 3.0.18 - Vue fixed adding user styles. Test added.
* 3.0.17 - Vue VmName_InitialLoaded and VmName_DataLoaded support added.
* 3.0.16 - Vue added to nuget package. Logo color #600060
* 3.0.15 - Vue custom elements support added.
* 3.0.14 - Vue callback support added.
* 3.0.13 - Support for user_DataLoaded added.
* 3.0.12 - Support for user_InitialLoaded added.
* 3.0.11 - Support for controller extension vmname_user.js added as in stonehenge 1.
* 3.0.10 - Initial Vue VM support - no callbacks yet.
* 3.0.9 - Started working on Vue support.
* 3.0.8 - Added all remaining tests from V2.
* 3.0.7 - Added more tests. Fixed IResourceProvider context item
* 3.0.6 - Tests using different file names
* 3.0.5 - nuspec updated
* 3.0.4 - FAKE build script updated. Stuck in System.InvalidOperationException: Unknown test framework: could not find xunit.dll (v1) or xunit.execution.*.dll (v2) in C:\ICT Baden\stonehenge3\tests...
* 3.0.3 - Added initial resource loader tests.
* 3.0.2 - Added option DisableSessionIdUrlParameter to host.
* 3.0.1 - Fixed chrome start windows
* 3.0.0 - Initial release

