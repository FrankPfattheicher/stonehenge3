
## Use IIS as Host for Stonehenge App

As a result you can use all IIS configuration options

* SSL offloading
* Windows-Authentication


### 1. Ensure Project Defaults

The project has to use the web SDK: Project Sdk="Microsoft.NET.Sdk.Web"

Add a `web.config` file as content.

Example:

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <location path="." inheritInChildApplications="false">
        <system.webServer>
          <handlers>
            <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
          </handlers>
          <aspNetCore processPath="dotnet" arguments=".\VueSampleCore.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
        </system.webServer>
      </location>
        <system.web>
            <identity impersonate="false" />
        </system.web>
    </configuration>

Replace "VueSampleCore.dll" with your application name.


### 2. Install the AspNetCoreModuleV2 Extension

Download here: https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-aspnetcore-3.1.10-windows-hosting-bundle-installer


### 3. Open the IIS Manager

* Create a new IIS website.
* Select the binary directory of the application as physical path.

