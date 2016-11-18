module Session

open Suave
open Suave.Operators
open Suave.State.CookieStateStore
open System

let UIDSTR = "userId"
  
let sessionState f g =
  context( fun r ->
    match HttpContext.state r with
    | None ->  g
    | Some store -> f store)

let sessionState' f = sessionState f (Redirection.redirect "logon")

let requiresAuthentication part =
  statefulForSession 
  >=> sessionState' (fun state ->
      match state.get UIDSTR with
      | Some userId when userId <> String.Empty ->  
        Console.WriteLine("User {0} logged on.",userId)
        part userId
      | _ -> 
        Redirection.redirect "logon") 
