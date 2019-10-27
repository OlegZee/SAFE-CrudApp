module Handlers.Auth

open Microsoft.AspNetCore.DataProtection
open Giraffe

open FSharp.Data
open FSharp.Control.Tasks.ContextInsensitive

open DataAccess
open ServerProtocol.V1

[<CLIMutable>]
type SessionData = {
    user_id: int
    user_role: string
}

module private Implementation =

    let requiresAuth handler : HttpHandler =
        fun (next : HttpFunc) ctx ->
            let protector = ctx.GetService<IDataProtectionProvider>().CreateProtector("login")
            (next, ctx) ||>
            match ctx.GetRequestHeader "Authorization" with
            | Ok token ->
                let encrypted = token.Substring 7
    
                match CryptoHelpers.tryDecrypt<SessionData> protector encrypted with  
                | Some session -> handler session
                | None -> RequestErrors.FORBIDDEN "bad token"
            | Error e -> RequestErrors.FORBIDDEN e
    
    let login : HttpHandler =
        fun next ctx -> task {
            let protector = ctx.GetService<IDataProtectionProvider>().CreateProtector("login")
            let! loginData = ctx.BindJsonAsync<LoginPayload>()
            let pwdHash = CryptoHelpers.calculateHash loginData.pwd

            match query
                { for user in dataCtx.Public.Users do
                    where (user.Login = loginData.login); select user } |> Seq.tryHead with

            | Some user when user.Pwdhash = pwdHash ->
                let sessionData = { user_id = user.Id; user_role = user.Role |> Option.defaultValue "user" }
                let token = CryptoHelpers.encrypt protector sessionData

                return! Successful.OK { token = token } next ctx
            | Some _ ->
                return! RequestErrors.FORBIDDEN "Pwd" next ctx
            | None ->
                return! RequestErrors.FORBIDDEN "Not Found" next ctx

        }

    let who : HttpHandler =
        requiresAuth(fun session ->
            match query
                { for user in dataCtx.Public.Users do
                    where (user.Id = session.user_id); select user } |> Seq.tryHead with
            | Some user ->
                Successful.OK { role = session.user_role; name = user.Name; login = user.Login }
            | None -> RequestErrors.NOT_FOUND "" )

    let changePwd : HttpHandler =
        requiresAuth(fun session next ctx ->
            task {
                let! payload = ctx.BindJsonAsync<ChPassPayload>()
                let oldpwdHash = CryptoHelpers.calculateHash payload.oldpwd
                let newpwdHash = CryptoHelpers.calculateHash payload.newpwd

                let foundUser = query {
                    for user in dataCtx.Public.Users do
                    where (user.Id = session.user_id && user.Pwdhash = oldpwdHash)
                    select (Some user);
                    exactlyOneOrDefault }
    
                match foundUser with
                | Some user ->
                    user.Pwdhash <- newpwdHash
                    dataCtx.SubmitUpdates()
                    return! Successful.NO_CONTENT next ctx
                | None ->
                    return! RequestErrors.FORBIDDEN "old password don't match" next ctx
            })
        
open Implementation

let requiresAuth = Implementation.requiresAuth

let handler : HttpHandler =
    choose [
        route "/api/login" >=> POST >=> login
        route "/api/who" >=> GET >=> who
        route "/api/chpass" >=> POST >=> changePwd
    ]