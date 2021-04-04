# stonehenge3
An open source .NET Framework to use Web UI technologies for desktop and/or web applications.

See a (very) short [getting started introduction here](docs/GettingStarted.md).

## Version V3.x
This version is based on .NET Core 3.1. 
The basing on .NET Standard 2.0 was replaced with version 3.16 because future versions of netcore no longer supports _Standard_.

Used technology

* [Kestrel](https://docs.microsoft.com/de-de/aspnet/core/fundamentals/servers/kestrel) - the Microsoft netcore web stack for self hosting
* [Vue.js 2](https://vuejs.org/) client framework
* [Bootstrap 4](https://getbootstrap.com/) front-end open source toolkit
* [Fontawesome 4](https://fontawesome.com/) icon library
* [Newtonsoft.JSON](https://www.newtonsoft.com/json) serializer for view models

Read the release history: [ReleaseNotes](ReleaseNotes3.md)

## Still there
* v3.6 - Aurelia client framework (deprecated, included up to v3.6 only)
* V2.x - (deprecated) .NET Full Framework V4.6, Katana, Aurelia
* V1.x - .NET Full Framework V4.6, ServiceStack, Knockout


**SampleFull with target framework V4.7.1**    
The application is able to use netstandard 2.0 libraries adding the following lines to lines to the csproj file.

	<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
  	<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>

