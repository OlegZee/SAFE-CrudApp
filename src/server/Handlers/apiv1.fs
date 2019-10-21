module Handlers.ApiV1

open System.Linq
open Giraffe
open FSharp.Data
open FSharp.Control.Tasks.ContextInsensitive

open DataAccess.SqlModel
open ServerProtocol.V1
open System

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

module private Implementation =

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

    // returns SummaryData
    let getUserSummary (userId: int): HttpHandler =
        fun next ctx ->
            query { 
                for record in dataCtx.Public.Calories do
                    where (record.UserId = userId)
                    groupBy record.ConsumeDate into g
                    select  {
                        rdate = g.Key
                        count = g.Count()
                        amount = g.Sum(fun g -> g.Amount) |> float }
                }
            |> ctx.WriteJsonAsync

    let getUserDataAt (userId, y, m, d) : HttpHandler =
        let rdate = DateTime(y, m, d)
        fun next ctx ->
            query { 
                for record in dataCtx.Public.Calories do
                    where (record.UserId = userId && record.ConsumeDate = rdate)
                    select (Mappings.userData record)
            } |> Seq.toList |> function
            | [] ->   RequestErrors.NOT_FOUND "Not Found" next ctx
            | data -> ctx.WriteJsonAsync data
            
    let postUserData (userId, y, m, d) : HttpHandler =
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

    let putUserData (userId, y, m, d, recordId) : HttpHandler =
        fun next ctx -> task {
            let foundRecordMaybe = query {
                for c in dataCtx.Public.Calories do
                where (c.UserId = userId && c.Id = recordId && c.ConsumeDate = DateTime(y, m, d))
                select (Some c)
                exactlyOneOrDefault
            }
            let! newData = ctx.BindJsonAsync<UpdUserData>()
            printfn "put %A" newData

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

            
    let deleteUserData (userId, y, m, d, recordId) : HttpHandler =
        let rdate = DateTime(y, m, d)
        fun next ctx ->
            task {
                let deleteQuery = query {
                    for c in dataCtx.Public.Calories do
                    where (c.UserId = userId && c.Id = recordId && c.ConsumeDate = rdate)
                }
                let resultCode = Seq.length deleteQuery |> function
                    |0 -> Successful.OK | _ -> RequestErrors.NOT_FOUND
                let! _ = deleteQuery |> Sql.Seq.``delete all items from single table``

                return! resultCode "" next ctx
            }
            
open Implementation
            
let handler : HttpHandler =
    choose [
        route "/users" >=> GET >=> users
        routef "/users/%i" (fun u -> GET >=> getUser u)
        GET >=> (routef "/users/%i/data" getUserSummary)
        GET >=> (routef "/users/%i/data/%i-%i-%i" getUserDataAt)
        POST >=> (routef "/users/%i/data/%i-%i-%i" postUserData)
        DELETE >=> (routef "/users/%i/data/%i-%i-%i/%i" deleteUserData)
        PUT >=> (routef "/users/%i/data/%i-%i-%i/%i" putUserData)
    ]
