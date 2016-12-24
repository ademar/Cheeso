module Globals

open System
open Suave.Logging

let emptyArrSeg = ArraySegment<byte>([||]);

let logger = Targets.create Verbose [| "Cheeso" |]