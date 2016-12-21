module Database

open Model
open System
open System.Data.Common
open Suave.Data
open MySql.Data.MySqlClient

let databaseConnectionString =
  let _str = System.Environment.GetEnvironmentVariable("DB_CON_STR")
  if String.IsNullOrEmpty _str then
    "host=localhost; database=chesso; user=chesso"
  else _str

let open_connection () =
  let cn = new MySqlConnection(databaseConnectionString) :> DbConnection
  cn.Open()
  cn

let saveUser (user: User) =
  use cn = open_connection ()
  let tx = sql cn
  let createdOn = user.createdOn.ToString("yyyy-MM-dd H:mm:ss")
  tx.Query "INSERT INTO users (id,displayName,email,encryptedPassword,salt,createdOn,lastSeen) VALUES (%s,%s,%s,%s,%s,%s,%s)" 
    user.id user.displayName user.email user.encryptedPassword user.salt createdOn createdOn
  |> executeNonQuery

let selectUserByEmail (email:string) : User option =
  use cn = open_connection ()
  let tx = sql cn
  tx { 
    let! q = tx.Query "SELECT * FROM users WHERE email=%s" email
    return q }
