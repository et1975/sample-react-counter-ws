// include Fake libs
#r "./packages/build/FAKE/tools/FakeLib.dll"
#r "System.IO.Compression.FileSystem"

open System
open System.IO
open Fake
open Fake.NpmHelper
open Fake.Git


let yarn = 
    if EnvironmentHelper.isWindows then "yarn.cmd" else "yarn"
    |> ProcessHelper.tryFindFileOnPath
    |> function
       | Some yarn -> yarn
       | ex -> failwith ( sprintf "yarn not found (%A)\n" ex )

let gitName = "sample-react-counter-ws"
let gitOwner = "fable-elmish"
let gitHome = sprintf "https://github.com/%s" gitOwner

// Filesets
let projects  =
      !! "src/*/*.fsproj"


let dotnetcliVersion = "1.0.1"
let mutable dotnetExePath = "dotnet"

let runDotnet workingDir =
    DotNetCli.RunCommand (fun p -> { p with ToolPath = dotnetExePath
                                            WorkingDir = workingDir } )

Target "InstallDotNetCore" (fun _ ->
   dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "Clean" (fun _ ->
    CleanDir "build"
)

Target "Install" (fun _ ->
    Npm (fun p ->
        { p with
            NpmFilePath = yarn
            Command = Install Standard
        })
    projects
    |> Seq.iter (fun s -> 
        let dir = IO.Path.GetDirectoryName s
        runDotnet dir "restore"
    )
)

Target "Build" (fun _ ->
    projects
    |> Seq.iter (fun s -> 
        let dir = IO.Path.GetDirectoryName s
        runDotnet dir "fable yarn-run build")
)

Target "Watch" (fun _ ->
    projects
    |> Seq.head
    |> (fun s -> 
        let dir = IO.Path.GetDirectoryName s
        runDotnet dir "fable yarn-run start")
)


// Build order
"Clean"
  ==> "InstallDotNetCore"
  ==> "Install"
  ==> "Build"

"Clean"
  ==> "InstallDotNetCore"
  ==> "Install"
  ==> "Watch"

  
// start build
RunTargetOrDefault "Build"
