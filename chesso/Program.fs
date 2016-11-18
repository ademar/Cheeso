
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Logging
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open Suave.Json
open System
open System.Net

let logger = Targets.create Verbose

open Model

let wsHandler (webSocket : WebSocket) =
  fun cx -> socket {
    let loop = ref true
    while !loop do
      let! msg = webSocket.read()
      match msg with
      | (Text, data, true) ->
        let str = UTF8.toString data
        let move = fromJson<Message> data
        // relay move to adversary
        Console.WriteLine str
      | (Ping, _, _) ->
        do! webSocket.send Pong (ArraySegment([||])) true
      | (Close, _, _) ->
        do! webSocket.send Close (ArraySegment([||])) true
        loop := false
      | _ -> ()
  }

let app : WebPart =
  choose [
    path "/websocket" >=> handShake wsHandler
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

