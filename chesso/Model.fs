
module Model

open System
open System.Runtime.Serialization

[<DataContract>]
type Move = {
  [<field: DataMember(Name = "color")>]
  color : string;
  [<field: DataMember(Name = "from")>]
  from  : string;
  [<field: DataMember(Name = "to")>]
  ``to``    : string;
  [<field: DataMember(Name = "flags")>]
  flags : string;
  [<field: DataMember(Name = "piece")>]
  piece : string;
  [<field: DataMember(Name = "san")>]
  san   : string; // Standard Algebraic Notation
  }

[<DataContract>]
type Message = {
  [<field: DataMember(Name = "move")>]
  move : Move;
  [<field: DataMember(Name = "board")>]
  board    : string;
  }

type Game = { 
  id : string
  wid    : string
  bid    : string
  wes    : Event<Message>
  bes    : Event<Message> }

type User = { 
  id : string
  uid : int
  displayName : string
  email : string
  encryptedPassword : string
  salt : string
  createdOn : DateTime
  lastSeen : DateTime }