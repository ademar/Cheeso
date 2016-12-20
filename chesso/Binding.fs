module Binding

open Suave

let getKey x f =
  request( fun req ->
    match req.formData x with
    | Choice1Of2 v -> f v
    | Choice2Of2 e -> RequestErrors.BAD_REQUEST e)

let (>>.) a f = getKey a f