module Channel

open System
open System.Threading

open Suave
open Suave.Json
open Suave.WebSocket
open Suave.Sockets
open Suave.Sockets.Control
open Suave.Logging
open Suave.Logging.Message

open Globals
open Model
open Games

let wsHandler userId gameId (webSocket : WebSocket) (cx : HttpContext) =
  let (weMoved,theyMoved) = events gameId userId
  let loop = ref true
  let notifyLoop = 
    async { 
      while !loop do
        let! msg = 
          Async.AwaitEvent theyMoved.Publish
        let bts = toJson msg
        let txt = UTF8.toString bts
        logger.verbose <| fun x -> event x (sprintf "Received:%s" txt)
        // relay adversary move upstream
        let! u = webSocket.send Text (ArraySegment(bts)) true
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
        logger.logSimple(event Verbose (sprintf "sent:%s" str))
      | (Ping, _, _) ->
        do! webSocket.send Pong emptyArrSeg true
      | (Close, _, _) ->
        do! webSocket.send Close emptyArrSeg true
        loop := false
      | _ -> ()
  }