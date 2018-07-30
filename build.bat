cls
IF NOT EXIST "packages\FAKE\tools\Fake.exe" ".\NuGet.exe" Install "FAKE" -OutputDirectory "packages" -ExcludeVersion
IF NOT EXIST "packagespackages\xunit.runner.console\tools\netcoreapp2.0\xunit.console.dll" ".\NuGet.exe" Install "xunit.runner.console" -OutputDirectory "packages" -ExcludeVersion
IF NOT EXIST "packages\Fake.DotNet.Testing.XUnit2\Fake.DotNet.Testing.XUnit2.nupkg" ".\NuGet.exe" Install "Fake.DotNet.Testing.XUnit2" -OutputDirectory "packages"
"packages\FAKE\tools\Fake.exe" build.fsx
