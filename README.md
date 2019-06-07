# stonehenge3
An open source .NET Framework to use Web UI technologies for desktop and/or web applications.

See a (very) short [getting started introduction here](docs/GettingStarted.md).

## Version V3.0
This version is based on .NET Standard 2.0.

* Kestrel - the Microsoft web stack for self hosting
* Vue client framework
* Aurelia client framework (deprecated)
* Newtonsoft.JSON serializer for view models

## Still there 
* V2.0 - (deprecated) .NET Full Framework V4.6, Katana, Aurelia
* V1.x - .NET Full Framework V4.6, ServiceStack, Knockout


## SampleFull with target framework V4.7.1
The application is able to use netstandard 2.0 libraries adding the following lines to lines to the csproj file.

	<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
  	<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>

