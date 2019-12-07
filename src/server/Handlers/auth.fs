module Handlers.Auth

open System.Security.Claims
open Microsoft.AspNetCore.Authentication

open Giraffe

open FSharp.Control.Tasks.ContextInsensitive

open DataAccess
open ServerProtocol.V1
open System

[<CLIMutable>]
type SessionData = {
    user_id: int
    user_role: string
}

module private Implementation =

    let DefaultAuthSchema = "Cookies"

    let isEmpty = String.IsNullOrWhiteSpace
    let ClaimUserId = "userid"

    // TODO it used to return 401 which causes login dialog to appear in browser
    let notLoggedIn = RequestErrors.FORBIDDEN "You must be logged in."
    let notProUserOrAdmin x = RequestErrors.FORBIDDEN "Permission denied. You must be a pro user or admin." x

    let mustBeLoggedIn x = requiresAuthentication notLoggedIn x            

    let requiresAuth handler : HttpHandler =
        mustBeLoggedIn >=>
        fun (next : HttpFunc) ctx ->

            let claims = ctx.User.Claims |> Seq.map (fun c -> c.Type, c.Value) |> Map.ofSeq
            match claims |> Map.tryFind ClaimTypes.Role, claims |> Map.tryFind ClaimUserId |> Option.map Int32.TryParse with
            | Some role, Some (true, userId) ->
                handler { user_id = userId; user_role = role } next ctx
            | _ ->
                ServerErrors.INTERNAL_ERROR "Failed to get claims" next ctx

    let login : HttpHandler =
        fun next ctx -> task {
            let! loginData = ctx.BindJsonAsync<LoginPayload>()
            let pwdHash = CryptoHelpers.calculateHash loginData.pwd

            match query
                { for user in dataCtx.Public.Users do
                    where (user.Login = loginData.login); select user } |> Seq.tryHead with

            | Some user when user.Pwdhash = pwdHash ->
                let properties = AuthenticationProperties()
                properties.IsPersistent <- true
                properties.ExpiresUtc <- DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(30.)) |> Nullable

                let claims = 
                    [   Claim (ClaimUserId, string user.Id)
                        Claim (ClaimTypes.Role, user.Role |> Option.defaultValue "user")  ]
                let principal = ClaimsPrincipal(ClaimsIdentity(claims, DefaultAuthSchema))
                
                do! ctx.SignInAsync principal
                return! Successful.OK () next ctx
            | _ ->
                return! RequestErrors.FORBIDDEN () next ctx
        }

    let signup : HttpHandler =
        fun next ctx ->
            task {
                let! info = ctx.BindJsonAsync<SignupPayload>()
                // FIXME case insensitive
                let isExistingUser = query { for record in dataCtx.Public.Users do exists (record.Login = info.login) }

                if isExistingUser then
                    return! RequestErrors.CONFLICT {| error = "User with such login already exists" |} next ctx
                else if isEmpty info.login || isEmpty info.name then
                    return! RequestErrors.FORBIDDEN {| error = "Name and login cannot be empty" |} next ctx
                else
                    let record = dataCtx.Public.Users.Create()
                    record.Login <- info.login
                    record.Name <- info.name
                    record.Role <- Some "user"
                    record.TargetExpenses <- Some (decimal 0)
                    record.Pwdhash <- CryptoHelpers.calculateHash info.pwd

                    do! dataCtx.SubmitUpdatesAsync()

                    return! Successful.OK "{}" next ctx // FIXME change to NO_CONTENT (needs unit support in Thoth)
            }

    let who : HttpHandler =
        requiresAuth(fun session ->
            match query
                { for user in dataCtx.Public.Users do
                    where (user.Id = session.user_id); select user } |> Seq.tryHead with
            | Some user ->
                Successful.OK { role = session.user_role; name = user.Name; login = user.Login }
            | None -> ServerErrors.INTERNAL_ERROR "Cannot get here" )

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
        route "/api/signup" >=> POST >=> signup
        route "/api/signout" >=> POST >=> signOut DefaultAuthSchema
        route "/api/who" >=> GET >=> who
        route "/api/chpass" >=> POST >=> changePwd
    ]