﻿
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
    pathScan "/game/%s" (fun gameId -> requiresAuthentication(fun userId -> Games.gamePage userId gameId))
    pathScan "/websocket/%s" (fun gameId -> requiresAuthentication(fun userId -> handShake (Channel.wsHandler userId gameId)))
    pathScan "/join/%s" Games.joinGame
    pathRegex "(.*?)\.(fsx|dll|mdb|log|chtml)$" >=> RequestErrors.FORBIDDEN "Access denied.";
    GET >=> choose [
      path "/signup" >=> razor "signup" null
      path "/signin" >=> razor "signin" null
      path "/jscripts" >=> Minify.jsBundle ["/js/jquery-3.1.1.min.js"; "/js/jquery.json.min.js"; "/js/chess.js"; "/js/chessboard-0.3.0.js"; "/js/app.js"]
      path "/" >=> razor "default" (Seq.map (fun (a) -> a) Games.games.Values);
      Files.browseHome
      ]
    POST >=> choose [
      path "/createGame" >=> requiresAuthentication Games.createGame
      path "/signup" >=> Games.createUser
      path "/signin" >=> Games.logonUser
      ]
    RequestErrors.NOT_FOUND "File not found."
    ] >=> log logger logFormat;

let config port = 
    { defaultConfig with
       bindings = 
         [ { scheme = HTTP ; socketBinding = { ip = IPAddress.Parse "0.0.0.0" ; port = port }}; ]
       logger = Targets.create Verbose [| "Suave" |]
    }

[<EntryPoint>]
let main argv = 
  let _port = System.Environment.GetEnvironmentVariable("PORT")
  let port = 
    if _port = null then 3000us
    else Convert.ToUInt16 _port
  startWebServer (config port) app
  0

