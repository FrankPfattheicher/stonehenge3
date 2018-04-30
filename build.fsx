
#r @"packages\FAKE\tools\FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
open System.IO

let NUnit = Fake.Testing.NUnit3.NUnit3

let release =
    ReadFile "ReleaseNotes3.md"
    |> ReleaseNotesHelper.parseReleaseNotes

// Properties
let buildDir46 = @".\builds46"
let testsDir = @".\tests"
let artifactsDir = @".\Artifacts"

let CreateDirs dirs = for dir in dirs do CreateDir dir

Target "Clean" (fun _ ->
    CreateDirs [artifactsDir]
    CleanDirs [artifactsDir]
    CleanDirs [testsDir]
    CleanDirs [buildDir46]
)

Target "CreatePackage" (fun _ ->
    // Copy all the package files into a package folder
    let libFile46 = buildDir46 </> @"IctBaden.Stonehenge3.dll"
    if Fake.FileHelper.TestFile libFile46
    then CleanDir @".\nuget"
         CreateDir @".\nuget\lib" 
         CreateDir @".\nuget\lib\net46" 
         CopyFiles @".\nuget\lib\net46" [ libFile46; 
                                          buildDir46 </> @"IctBaden.Stonehenge3.pdb";
                                          buildDir46 </> @"IctBaden.Stonehenge3.Aurelia.dll"; 
                                          buildDir46 </> @"IctBaden.Stonehenge3.Aurelia.pdb"; 
                                          buildDir46 </> @"IctBaden.Stonehenge3.Kestrel.dll"; 
                                          buildDir46 </> @"IctBaden.Stonehenge3.Kestrel.pdb"; 
                                          buildDir46 </> @"IctBaden.Stonehenge3.SimpleHttp.dll"; 
                                          buildDir46 </> @"IctBaden.Stonehenge3.SimpleHttp.pdb"
                                        ]
         NuGet (fun p -> 
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
                      (@"lib/net46/IctBaden.Stonehenge3.dll", Some "lib/net46", None)
                      (@"lib/net46/IctBaden.Stonehenge3.pdb", Some "lib/net46", None) 
                      (@"lib/net46/IctBaden.Stonehenge3.Aurelia.dll", Some "lib/net46", None)
                      (@"lib/net46/IctBaden.Stonehenge3.Aurelia.pdb", Some "lib/net46", None) 
                      (@"lib/net46/IctBaden.Stonehenge3.Kestrel.dll", Some "lib/net46", None)
                      (@"lib/net46/IctBaden.Stonehenge3.Kestrel.pdb", Some "lib/net46", None) 
                      (@"lib/net46/IctBaden.Stonehenge3.SimpleHttp.dll", Some "lib/net46", None)
                      (@"lib/net46/IctBaden.Stonehenge3.SimpleHttp.pdb", Some "lib/net46", None) 
                    ]
            ReferencesByFramework = [ { FrameworkVersion  = "net46"; References = [ "IctBaden.Stonehenge3.dll"; 
                                                                                    "IctBaden.Stonehenge3.Aurelia.dll";
                                                                                    "IctBaden.Stonehenge3.Kestrel.dll";
                                                                                    "IctBaden.Stonehenge3.SimpleHttp.dll"
                                                                                  ] } ]
            DependenciesByFramework = [ { FrameworkVersion  = "net46"; Dependencies = [ "Microsoft.Owin", "3.1.0";
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

Target "Build46" (fun _ ->
     !! @".\**\*.csproj" 
     -- @".\**\*Test.csproj"
      |> MSBuildRelease buildDir46 "Build"
      |> Log "Build-Output: "
)

Target "Build46Tests" (fun _ ->
     !! @".\**\*Test.csproj"
      |> MSBuildRelease testsDir "Build"
      |> Log "TestBuild-Output: "
)

Target "Run46Tests" (fun _ ->
    !! (testsDir + @"\*Test.dll")
      |> NUnit (fun p -> 
      {p with 
        ShadowCopy = false
        WorkingDir = artifactsDir
      })
)

// Dependencies
"Clean"
  ==> "Build46"
  ==> "Build46Tests"
  ==> "Run46Tests"
  ==> "CreatePackage"

RunTargetOrDefault "CreatePackage"
