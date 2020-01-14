module Handlers.ApiV1

open System
open System.Linq

open Giraffe
open FSharp.Data
open FSharp.Control.Tasks.ContextInsensitive

open DataAccess
open ServerProtocol.V1

module private Mappings =

    let user (d: PostgreSqlExpenses.dataContext.``public.usersEntity``) =
        {   user_id = d.Id
            name = d.Name
            login = d.Login
            expenseLimit =  d.TargetExpenses |> (Option.map float >> Option.defaultValue 0.0)
            role = d.Role |> Option.defaultValue "user" }

    let userData (r: PostgreSqlExpenses.dataContext.``public.expensesEntity``): UserData =
        {
            record_id = r.Id;
            rtime = r.SpendTime.ToString("hh':'mm")
            item = r.Item
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
                let! info = ctx.BindJsonAsync<CreateUserPayload>()
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
                    record.TargetExpenses <- Some (decimal info.expenseLimit)
                    record.Pwdhash <- CryptoHelpers.calculateHash info.pwd

                    do! dataCtx.SubmitUpdatesAsync()

                    return! Successful.CREATED ({ user_id = record.Id }) next ctx
            }

    let updateUser userId : HttpHandler =
        fun next ctx -> task {
            let! payload = ctx.BindJsonAsync<UpdateUserPayload>()
            let result =
                query {
                    for u in dataCtx.Public.Users do
                    where (u.Id = userId); select u }
                |> Seq.tryHead
                |> function
                | Some data ->
                    data.Login <- payload.login
                    data.Name <- payload.name
                    data.Role <- Some payload.role
                    data.TargetExpenses <-
                        payload.expenseLimit |> function| a when a > 0.0 ->  Some <| decimal a | _ -> None
                    dataCtx.SubmitUpdates()
                    Successful.OK ""
                | None ->
                    RequestErrors.NOT_FOUND "Not Found"
            return! result next ctx
        }

    let deleteUser userId  : HttpHandler =
        fun next ctx ->
            let reportDbError text = RequestErrors.CONFLICT {| error = text |} next ctx
            task {
                let deleteQuery = query { for c in dataCtx.Public.Users do where (c.Id = userId) }
                let resultCode = 
                    match deleteQuery |> Seq.tryHead with
                    | Some _ -> Successful.NO_CONTENT
                    | _ -> RequestErrors.NOT_FOUND ""
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
            let tryGet name parse deflt = 
                match ctx.TryGetQueryStringValue name |> Option.map parse with
                | Some (true, value) -> value | _ -> deflt

            let dateFrom =  tryGet "from" DateTime.TryParse DateTime.MinValue
            let dateTo =  tryGet "to" DateTime.TryParse DateTime.MaxValue

            let timeFrom =  tryGet "tfrom" TimeSpan.TryParse TimeSpan.MinValue
            let timeTo =  tryGet "tto" TimeSpan.TryParse TimeSpan.MaxValue

            // TODO build query on the fly

            query { 
                for record in dataCtx.Public.Expenses do
                    where (record.UserId = userId
                        && record.SpendDate >= dateFrom && record.SpendDate <= dateTo
                        && record.SpendTime >= timeFrom && record.SpendTime <= timeTo )
                    groupBy record.SpendDate into g
                    sortBy g.Key
                    select  {
                        rdate = g.Key
                        count = g.Count()
                        amount = g.Sum(fun g -> float g.Amount) }
                }
            |> ctx.WriteJsonAsync

    let getUserDataAt userId (y, m, d) : HttpHandler =
        let rdate = DateTime(y, m, d)
        fun next ctx ->
            if query { for record in dataCtx.Public.Users do exists (record.Id = userId) } then
                query { 
                    for record in dataCtx.Public.Expenses do
                        where (record.UserId = userId && record.SpendDate = rdate)
                        sortBy record.SpendTime; thenBy record.Id
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
                    let! newData = ctx.BindJsonAsync<PostDataPayload>()
                    let record = dataCtx.Public.Expenses.Create()
                    // TODO validate data
                    record.UserId <- userId
                    record.SpendDate <- DateTime(y, m, d)
                    record.SpendTime <- TimeSpan.Parse newData.rtime
                    record.Item <- newData.item
                    record.Amount <- decimal newData.amount
                    do! dataCtx.SubmitUpdatesAsync()

                    return! Successful.CREATED ({ record_id = record.Id }) next ctx
                }

    let putUserData userId (y, m, d, recordId) : HttpHandler =
        fun next ctx -> task {
            let foundRecordMaybe = query {
                for c in dataCtx.Public.Expenses do
                where (c.UserId = userId && c.Id = recordId && c.SpendDate = DateTime(y, m, d))
                select (Some c)
                exactlyOneOrDefault
            }
            let! newData = ctx.BindJsonAsync<PostDataPayload>()

            return!
                match foundRecordMaybe with
                | Some dataRecord ->
                    match newData.amount with
                    | a when a > 0.0 -> dataRecord.Amount <- decimal a | _ -> ()

                    match newData.item with
                    | null | "" -> () | s -> dataRecord.Item <- s

                    match TimeSpan.TryParse newData.rtime with
                    | true, value -> dataRecord.SpendTime <- value
                    | _ -> ()

                    dataCtx.SubmitUpdates()
                    Successful.NO_CONTENT next ctx
                | None ->
                    RequestErrors.NOT_FOUND "Not Found"  next ctx
        }

    let deleteUserData userId (y, m, d, recordId) : HttpHandler =
        let rdate = DateTime(y, m, d)
        fun next ctx ->
            task {
                let deleteQuery = query {
                    for c in dataCtx.Public.Expenses do
                    where (c.UserId = userId && c.Id = recordId && c.SpendDate = rdate)
                }
                let resultCode = Seq.length deleteQuery |> function
                    |1 -> Successful.NO_CONTENT
                    | _ -> RequestErrors.NOT_FOUND ""
                let! _ = deleteQuery |> Sql.Seq.``delete all items from single table``

                return! resultCode next ctx
            }

module SettingsApi =
    let getSettings userId : HttpHandler =
        fun next ctx ->
            query { 
                for user in dataCtx.Public.Users do
                    where (user.Id = userId); select user }
            |> Seq.tryHead
            |> function
            | Some user -> ctx.WriteJsonAsync { expenseLimit = user.TargetExpenses |> Option.map float |> Option.defaultValue 0.0 }
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
                    dataRecord.TargetExpenses <-
                        settings.expenseLimit |> function| a when a > 0.0 ->  Some <| decimal a | _ -> None
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
                        PUT >=> routef "/%i" UsersApi.updateUser
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
