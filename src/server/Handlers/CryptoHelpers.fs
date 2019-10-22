module Handlers.CryptoHelpers
// authentication related code

open System
open System.Text
open System.Text.Json
open System.Security.Cryptography
open Microsoft.AspNetCore.DataProtection

let calculateHash (input: string) =
    use algorithm = SHA256.Create() //or MD5 SHA256 etc.
    let hashedBytes = input |> Encoding.UTF8.GetBytes |> algorithm.ComputeHash
    BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

let encryptStr (protector: IDataProtector) (plaintext: string) =
    protector.Protect plaintext

let encrypt protector =
    JsonSerializer.Serialize >> encryptStr protector

let tryDecryptStr (protector: IDataProtector) encryptedText : string option =
    try (encryptedText : string) |> protector.Unprotect |> Some
    with | :? CryptographicException -> None

let tryDecrypt<'t> protector =
    tryDecryptStr protector >> Option.map JsonSerializer.Deserialize<'t>

