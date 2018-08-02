cls
IF NOT EXIST "packages\FAKE\tools\Fake.exe" ".\NuGet.exe" Install "FAKE" -OutputDirectory "packages" -ExcludeVersion
".\NuGet.exe" Install "Fake.DotNet.Testing.VSTest" -OutputDirectory "packages"
".\NuGet.exe" Install "xunit.runner.console" -OutputDirectory "packages"

"packages\FAKE\tools\Fake.exe" build.fsx
