module Utils

open System

let SALT_LENGTH = 32

let randomStr = 
  let chars = "abcdefghijklmnopqrstuvwuxyz0123456789"
  let charsLen = chars.Length
  let random = Random()

  fun len -> 
    let randomChars = [|for i in 0..(len - 1) -> chars.[random.Next(charsLen)]|]
    new String(randomChars)

open System.Text
open System.Security.Cryptography

let hashUserPassword (pwd: string) =
  let salt = randomStr SALT_LENGTH
  let bs = Encoding.UTF8.GetBytes (salt + pwd)
  use hasher = new SHA256Managed()
  let bss = hasher.ComputeHash(bs)
  Encoding.UTF8.GetString bss, salt