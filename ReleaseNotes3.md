
* 3.8.4 - Fixed initial binding errors due to initialize with null, now {}.
* 3.8.3 - Fixed custom elements with multiple bindings code generation.
* 3.8.2 - Fixed Vue warning missing key in routes. Fixed used assemblies with kestrel host.
* 3.8.1 - Fixed Vue component creation from ViewModel with dependency injection.
* 3.8.0 - Updated to Vue 2.6.10, Bootsrap-vuw 2.0.0, Bootstrap 4.3.1, {.min} support added.
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

