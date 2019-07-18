[<RequireQualifiedAccess>]
module FakeBuild

#r @"packages\FAKE\tools\FakeLib.dll"
#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet.Testing.VSTest //"

open Fake
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Fake.DotNet.Testing.VSTest

let release =
    File.read "ReleaseNotes3.md"
    |> Fake.Core.ReleaseNotes.parse

// Properties
let buildDir = @".\builds"
let testsDir = @".\tests"
let artifactsDir = @".\artifacts"

let CreateDirs dirs = for dir in dirs do Directory.create dir

Fake.Core.Target.create  "Clean" (fun _ ->
    CreateDirs [artifactsDir]
    Shell.cleanDirs [artifactsDir]
    Shell.cleanDirs [testsDir]
    Shell.cleanDirs [buildDir]
)

Fake.Core.Target.create "CreatePackage" (fun _ ->
    // Copy all the package files into a package folder
    let libFile = buildDir </> @"IctBaden.Stonehenge3.dll"
    if Shell.testFile libFile
    then Shell.cleanDir @".\nuget"
         Directory.create @".\nuget\lib" 
         Directory.create @".\nuget\lib\netstandard2.0" 
         Shell.copyFiles @".\nuget\lib\netstandard2.0" [ libFile; 
                                          buildDir </> @"IctBaden.Stonehenge3.pdb";
                                          buildDir </> @"IctBaden.Stonehenge3.Kestrel.dll"; 
                                          buildDir </> @"IctBaden.Stonehenge3.Kestrel.pdb"; 
                                          buildDir </> @"IctBaden.Stonehenge3.SimpleHttp.dll"; 
                                          buildDir </> @"IctBaden.Stonehenge3.SimpleHttp.pdb"
                                        ]
         Fake.DotNet.NuGet.NuGet.NuGet (fun p -> 
        {p with
            Authors = [ "Frank Pfattheicher" ]
            Project = "IctBaden.Stonehenge3"
            Description = "Web Application Framework"
            OutputPath = @".\nuget"
            Summary = "Web Application Framework"
            WorkingDir = @".\nuget"
            Version = release.NugetVersion
            ReleaseNotes = release.Notes.Head
            Files = [ 
                      (@"lib/netstandard2.0/IctBaden.Stonehenge3.dll", Some "lib/net", None)
                      (@"lib/netstandard2.0/IctBaden.Stonehenge3.pdb", Some "lib/net", None) 
                      (@"lib/netstandard2.0/IctBaden.Stonehenge3.Kestrel.dll", Some "lib/net", None)
                      (@"lib/netstandard2.0/IctBaden.Stonehenge3.Kestrel.pdb", Some "lib/net", None) 
                      (@"lib/netstandard2.0/IctBaden.Stonehenge3.SimpleHttp.dll", Some "lib/net", None)
                      (@"lib/netstandard2.0/IctBaden.Stonehenge3.SimpleHttp.pdb", Some "lib/net", None) 
                    ]
            ReferencesByFramework = [ { FrameworkVersion  = "net"; References = [ "IctBaden.Stonehenge3.dll"; 
                                                                                    "IctBaden.Stonehenge3.Kestrel.dll";
                                                                                    "IctBaden.Stonehenge3.SimpleHttp.dll"
                                                                                  ] } ]
            DependenciesByFramework = [ { FrameworkVersion  = "net"; Dependencies = [ "Microsoft.Owin", "3.1.0";
                                                                                        "Microsoft.Owin.Diagnostics", "3.1.0";
                                                                                        "Microsoft.Owin.Host.HttpListener", "3.1.0";
                                                                                        "Microsoft.Owin.Hosting", "3.1.0";
                                                                                        "Microsoft.Owin.SelfHost", "3.1.0";
                                                                                        "Owin", "1.0";
                                                                                        "SqueezeMe", "1.3.33";
                                                                                        "Newtonsoft.Json", "10.0.2" ] } ]
            Publish = false }) // using website for upload
            @"Stonehenge3.nuspec"
    else
        printfn "*****************************************************" 
        printfn "Output file missing. Package built with RELEASE only." 
        printfn "*****************************************************" 
)

Fake.Core.Target.create "BuildAll" (fun _ ->
     !! @".\**\*.csproj" 
     -- @".\**\*Test.csproj"
      |> Fake.DotNet.MSBuild.runRelease id buildDir "Build"
      |> Fake.Core.Trace.logItems "Build-Output: "
)

Fake.Core.Target.create "BuildAllTests" (fun _ ->
     !! @".\**\*Test.csproj"
      |> Fake.DotNet.MSBuild.runRelease id testsDir "Build"
      |> Fake.Core.Trace.logItems "TestBuild-Output: "
)

Fake.Core.Target.create "RunAllTests" (fun _ ->
     !! (testsDir @@ @"\*Test.dll")
       |> Fake.DotNet.Testing.VSTest (fun p -> { p with SettingsPath = "Local.RunSettings" })
)


// Dependencies
"Clean"
  ==> "BuildAll"
  ==> "BuildAllTests"
  ==> "RunAllTests"
  ==> "CreatePackage"

Fake.Core.Target.runOrDefault "CreatePackage"

