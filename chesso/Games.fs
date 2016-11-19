module Games
open System
open System.Collections.Generic

open Model
open Session

open Suave
open Suave.Operators
open Suave.Razor

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

let monitor = new obj()

let tryJoin (game: Game) userId =
  fun () ->
    if String.IsNullOrEmpty game.bid then
      games.Remove game.id |> ignore
      games.Add(game.id, { game with bid = userId })

let joinGame gameId =
  requiresAuthentication 
    (fun userId ->
      lock monitor (tryJoin games.[gameId] userId)
      if String.IsNullOrEmpty games.[gameId].wid then
        Successful.OK (sprintf "Failed to join game %s" gameId)
      else
        Successful.OK (sprintf "Joined game %s" gameId))

let logOn =
  sessionState' (fun st -> 
    let userId = Guid.NewGuid().ToString()
    st.set UIDSTR userId >=> Successful.OK (sprintf "Welcome user: %s" userId))

type GamePageView = { game : Game; playerColor : string }

let gamePage userId gameId =
  let game = games.[gameId];
  let playerColor =
    if game.wid = userId then "white"
    else "black"
  razor "game" { game = game; playerColor = playerColor }