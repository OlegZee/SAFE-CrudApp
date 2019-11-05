module CryptoHelpers
// authentication related code

open System
open System.Text
open System.Security.Cryptography

let calculateHash (input: string) =
    use algorithm = SHA256.Create() //or MD5 SHA256 etc.
    let hashedBytes = input |> Encoding.UTF8.GetBytes |> algorithm.ComputeHash
    BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
