
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Logging

open System
open System.Net

let logger = Targets.create Verbose

let app : WebPart =
  choose [
    pathRegex "(.*?)\.(fsx|dll|mdb|log|chtml)$" >=> RequestErrors.FORBIDDEN "Access denied.";
    path "/" >=> Files.file "Views/default.html" ;
    Files.browseHome
    ] >=> log logger logFormat;

let config port = 
    { defaultConfig with
       bindings = 
         [ { scheme = HTTP ; socketBinding = { ip = IPAddress.Parse "0.0.0.0" ; port = port }}; ]
    }

[<EntryPoint>]
let main argv = 
  let port = if argv.Length>0 then Convert.ToUInt16(argv.[0]) else 3000us
  startWebServer (config port) app
  0

