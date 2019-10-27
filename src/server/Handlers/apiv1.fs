module Handlers.ApiV1

open System
open System.Linq

open Giraffe
open FSharp.Data
open FSharp.Control.Tasks.ContextInsensitive

open DataAccess
open ServerProtocol.V1

module private Mappings =

    let user (d: PostgreSqlCalories.dataContext.``public.usersEntity``) =
        {   user_id = d.Id
            name = d.Name
            login = d.Login
            targetCalories =  d.TargetCalories |> (Option.map float >> Option.defaultValue 0.0)
            role = d.Role |> Option.defaultValue "user" }

    let userData (r: PostgreSqlCalories.dataContext.``public.caloriesEntity``): UserData =
        {
            record_id = r.Id;
            rtime = r.ConsumeTime.ToString("hh':'mm")
            meal = r.Meal
            amount = float r.Amount
        }

module private UsersApi =
    let users: HttpHandler =
        fun next ctx -> task {
            let allUsers = dataCtx.Public.Users |> Seq.map Mappings.user
            return! ctx.WriteJsonAsync allUsers }

    let getUser (userId: int): HttpHandler =
        fun next ctx ->
            query { 
                for user in dataCtx.Public.Users do
                    where (user.Id = userId); select (Mappings.user user) }
            |> Seq.tryHead
            |> function
            | Some user -> ctx.WriteJsonAsync user
            | _ ->         RequestErrors.NOT_FOUND "Not Found" next ctx

    let validateRole role =
        match role with
        | "user" | "admin" | "manager" -> Some role
        | _ -> None
            
    let createUser : HttpHandler =
        fun next ctx ->
            task {
                let! info = ctx.BindJsonAsync<CreateUserInfo>()
                // FIXME case insensitive
                let isExistingUser = query { for record in dataCtx.Public.Users do exists (record.Login = info.login) }

                if isExistingUser then
                    return! RequestErrors.CONFLICT "User with such login already exists" next ctx
                else
                    let record = dataCtx.Public.Users.Create()
                    // TODO validate data
                    record.Login <- info.login
                    record.Name <- info.name
                    record.Role <- info.role |> validateRole
                    record.TargetCalories <- Some (decimal info.targetCalories)
                    record.Pwdhash <- CryptoHelpers.calculateHash info.pwd

                    do! dataCtx.SubmitUpdatesAsync()

                    return! Successful.CREATED ({ user_id = record.Id }) next ctx
            }

    let deleteUser userId  : HttpHandler =
        fun next ctx ->
            let reportDbError text = RequestErrors.CONFLICT {| error = text |} next ctx
            task {
                let deleteQuery = query { for c in dataCtx.Public.Users do where (c.Id = userId) }
                let resultCode = 
                    match deleteQuery |> Seq.tryHead with
                    | Some _ -> Successful.OK "{}" | _ -> RequestErrors.NOT_FOUND ""
                try
                    let! _ = deleteQuery |> Sql.Seq.``delete all items from single table``
                    return! resultCode next ctx
                with
                    | :? AggregateException as e -> return! reportDbError e.InnerException.Message
                    | e -> return! reportDbError e.Message
            }
            
module UserInputApi =
    // returns SummaryData
    let getUserSummary (userId: int): HttpHandler =
        fun next ctx ->
            query { 
                for record in dataCtx.Public.Calories do
                    where (record.UserId = userId)
                    groupBy record.ConsumeDate into g
                    sortBy g.Key
                    select  {
                        rdate = g.Key
                        count = g.Count()
                        amount = g.Sum(fun g -> g.Amount) |> float }
                }
            |> ctx.WriteJsonAsync

    let getUserDataAt userId (y, m, d) : HttpHandler =
        let rdate = DateTime(y, m, d)
        fun next ctx ->
            if query { for record in dataCtx.Public.Users do exists (record.Id = userId) } then
                query { 
                    for record in dataCtx.Public.Calories do
                        where (record.UserId = userId && record.ConsumeDate = rdate)
                        sortBy record.ConsumeTime; thenBy record.Id
                        select (Mappings.userData record)
                } |> Seq.toList |> ctx.WriteJsonAsync
            else
                RequestErrors.NOT_FOUND "Not Found" next ctx
            
    let postUserData userId (y, m, d) : HttpHandler =
        fun next ctx ->
            let isExistingUser =
                query { for record in dataCtx.Public.Users do exists (record.Id = userId) }

            if not isExistingUser then
                RequestErrors.NOT_FOUND "Not Found" next ctx
            else
                task {
                    let! newData = ctx.BindJsonAsync<CreateUserData>()
                    let record = dataCtx.Public.Calories.Create()
                    // TODO validate data
                    record.UserId <- userId
                    record.ConsumeDate <- DateTime(y, m, d)
                    record.ConsumeTime <- TimeSpan.Parse newData.rtime
                    record.Meal <- newData.meal
                    record.Amount <- decimal newData.amount
                    do! dataCtx.SubmitUpdatesAsync()

                    return! Successful.CREATED ({ record_id = record.Id }) next ctx
                }

    let putUserData userId (y, m, d, recordId) : HttpHandler =
        fun next ctx -> task {
            let foundRecordMaybe = query {
                for c in dataCtx.Public.Calories do
                where (c.UserId = userId && c.Id = recordId && c.ConsumeDate = DateTime(y, m, d))
                select (Some c)
                exactlyOneOrDefault
            }
            let! newData = ctx.BindJsonAsync<UpdUserData>()

            return!
                match foundRecordMaybe with
                | Some dataRecord ->
                    match newData.amount with
                    | a when a > 0.0 -> dataRecord.Amount <- decimal a | _ -> ()

                    match newData.meal with
                    | null | "" -> () | s -> dataRecord.Meal <- s

                    match TimeSpan.TryParse newData.rtime with
                    | true, value -> dataRecord.ConsumeTime <- value
                    | _ -> ()

                    dataCtx.SubmitUpdates()
                    Successful.OK "" next ctx
                | None ->
                    RequestErrors.NOT_FOUND "Not Found"  next ctx
        }

    let deleteUserData userId (y, m, d, recordId) : HttpHandler =
        let rdate = DateTime(y, m, d)
        fun next ctx ->
            task {
                let deleteQuery = query {
                    for c in dataCtx.Public.Calories do
                    where (c.UserId = userId && c.Id = recordId && c.ConsumeDate = rdate)
                }
                let resultCode = Seq.length deleteQuery |> function
                    |1 -> Successful.OK | _ -> RequestErrors.NOT_FOUND
                let! _ = deleteQuery |> Sql.Seq.``delete all items from single table``

                return! resultCode "" next ctx
            }

module SettingsApi =
    let getSettings userId : HttpHandler =
        fun next ctx ->
            query { 
                for user in dataCtx.Public.Users do
                    where (user.Id = userId); select user }
            |> Seq.tryHead
            |> function
            | Some user -> ctx.WriteJsonAsync { targetCalories = user.TargetCalories |> Option.map float |> Option.defaultValue 0.0 }
            | _ ->         RequestErrors.NOT_FOUND "Not Found" next ctx

    let putSettings userId : HttpHandler =
        fun next ctx -> task {
            let! settings = ctx.BindJsonAsync<UserSettings>()
            let result =
                query {
                    for u in dataCtx.Public.Users do
                    where (u.Id = userId); select u }
                |> Seq.tryHead
                |> function
                | Some dataRecord ->
                    dataRecord.TargetCalories <-
                        settings.targetCalories |> function| a when a > 0.0 ->  Some <| decimal a | _ -> None
                    dataCtx.SubmitUpdates()
                    Successful.OK ""
                | None ->
                    RequestErrors.NOT_FOUND "Not Found"
            return! result next ctx
        }
            
let private usersDataHandler userId : HttpHandler =
    choose [
        GET >=> route "" >=> UserInputApi.getUserSummary userId
        GET >=> routef "/%i-%i-%i" (UserInputApi.getUserDataAt userId)
        POST >=> routef "/%i-%i-%i" (UserInputApi.postUserData userId)
        DELETE >=> routef "/%i-%i-%i/%i" (UserInputApi.deleteUserData userId)
        PUT >=> routef "/%i-%i-%i/%i" (UserInputApi.putUserData userId)
    ]

let private requiresRole roles (handler: HttpHandler) : HttpHandler =
    Auth.requiresAuth(fun session ->
        if roles |> List.contains session.user_role then handler
        else RequestErrors.FORBIDDEN "Requires privileged role")

let handler : HttpHandler =
    choose [
        subRoute "/users" <|
            choose [
                requiresRole ["admin"; "manager"] 
                    (choose [
                        route "" >=> GET >=> UsersApi.users
                        route "" >=> POST >=> UsersApi.createUser
                        GET >=> routef "/%i" UsersApi.getUser
                        DELETE >=> routef "/%i" UsersApi.deleteUser ])
                requiresRole ["admin"]
                    (subRoutef "/%i/data" usersDataHandler)
            ]
        subRoute "/data" <|
            Auth.requiresAuth(fun session -> usersDataHandler session.user_id)
        route "/me" >=> GET >=>
            Auth.requiresAuth(fun session -> UsersApi.getUser session.user_id)
        route "/settings" >=>
            Auth.requiresAuth(fun session ->
                choose [
                    GET >=> SettingsApi.getSettings session.user_id
                    PUT >=> SettingsApi.putSettings session.user_id
                ])
    ]
