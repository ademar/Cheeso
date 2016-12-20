
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Logging

open Suave.WebSocket
open Suave.State.CookieStateStore
open Suave.Razor

open System
open System.Threading
open System.Net

open Globals
open Session

let app : WebPart =
  statefulForSession >=> choose [
    pathScan "/game/%s" (fun gameId -> requiresAuthentication(fun userId -> gamePage userId gameId))
    pathScan "/websocket/%s" (fun gameId -> requiresAuthentication(fun userId -> handShake (Channel.wsHandler userId gameId)))
    path "/logon" >=> logOn
    pathScan "/join/%s" joinGame
    pathRegex "(.*?)\.(fsx|dll|mdb|log|chtml)$" >=> RequestErrors.FORBIDDEN "Access denied.";
    GET >=> choose [
      path "/jscripts" >=> Minify.jsBundle ["/js/jquery-3.1.1.min.js"; "/js/jquery.json.min.js"; "/js/chess.js"; "/js/chessboard-0.3.0.js"; "/js/app.js"]
      path "/" >=> razor "default" (Seq.map (fun (a) -> a) games.Values);
      Files.browseHome
      ]
    POST >=> choose [
      path "/createGame" >=> requiresAuthentication Games.createGame    
      path "/createUser" >=> requiresAuthentication Users.createUser

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

