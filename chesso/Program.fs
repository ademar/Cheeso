
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Logging
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open Suave.Json
open Suave.State.CookieStateStore
open Suave.Razor
open System
open System.Collections.Generic
open System.Threading
open System.Net

let logger = Targets.create Verbose

open Model
open Session

let emptyArrSeg = ArraySegment([||]);

let games = new Dictionary<string,Game>()

let events gameId userId =
  let game = games.[gameId]
  if userId = game.wid then
    (game.wes,game.bes)
  elif userId = game.bid then
    (game.bes,game.wes)
  else
    failwith "Invalid userId"

let newGame userId =
  { id = Guid.NewGuid().ToString(); wid = userId; bid = ""; wes = new Event<_>(); bes = new Event<_>()}

let createGame userId =
    let game = newGame(userId)
    games.Add (game.id, game)
    Successful.OK (sprintf "Created game %s" game.id)

let joinGame gameId =
  requiresAuthentication 
    (fun userId ->
      let game = games.[gameId]
      games.Remove gameId |> ignore
      games.Add(gameId, { game with bid = userId })
      Successful.OK (sprintf "Joined game %s" gameId))

let logOn =
  sessionState' (fun st -> 
    let userId = Guid.NewGuid().ToString()
    st.set UIDSTR userId >=> Successful.OK (sprintf "Welcome user: %s" userId))

let wsHandler userId gameId (webSocket : WebSocket) (cx : HttpContext) =
  let (weMoved,theyMoved) = events gameId userId
  let loop = ref true
  let notifyLoop = 
    async { 
      while !loop do
        let! msg = 
          Async.AwaitEvent theyMoved.Publish
        // relay adversary move upstream
        let! u = webSocket.send Text (ArraySegment(toJson msg)) true
        match u with
        | Choice1Of2  () -> ()
        | Choice2Of2 err -> failwith (err.ToString())
        do ignore()
    }
  let cts = new CancellationTokenSource()
  let r = Async.Start(notifyLoop, cts.Token)
  socket {
    while !loop do
      let! msg = webSocket.read()
      match msg with
      | (Text, data, true) ->
        let str = UTF8.toString data
        let msg = fromJson<Message> data
        // relay our move to adversary
        weMoved.Trigger msg
        Console.WriteLine str
      | (Ping, _, _) ->
        do! webSocket.send Pong emptyArrSeg true
      | (Close, _, _) ->
        do! webSocket.send Close emptyArrSeg true
        loop := false
      | _ -> ()
  }

let app : WebPart =
  statefulForSession >=> choose [
    pathScan "/game/%s" (fun gameId -> razor "game" games.[gameId] )
    pathScan "/websocket/%s" (fun gameId -> requiresAuthentication (fun userId -> handShake (wsHandler userId gameId)))
    path "/create" >=> requiresAuthentication createGame
    path "/logon" >=> logOn
    pathScan "/join/%s" joinGame
    pathRegex "(.*?)\.(fsx|dll|mdb|log|chtml)$" >=> RequestErrors.FORBIDDEN "Access denied.";
    GET >=> choose [
      path "/" >=> razor "default" (Seq.map (fun (a) -> a) games.Values);
      Files.browseHome
      ]
    RequestErrors.NOT_FOUND "File not found."
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

