module Handlers.ApiV1

open System.Linq
open Giraffe
open FSharp.Control.Tasks.ContextInsensitive

open DataAccess.SqlModel
open ServerProtocol.V1

module private Mappings =

    let user (d: PostgreSqlCalories.dataContext.``public.usersEntity``) =
        {   user_id = d.Id
            name = d.Name
            login = d.Login
            targetCalories =  d.TargetCalories |> (Option.map float >> Option.defaultValue 0.0)
            role = d.Role |> Option.defaultValue "user" }

    let userData (r: PostgreSqlCalories.dataContext.``public.caloriesEntity``) =
        {
            record_id = r.Id;
            rdate = r.ConsumeDate
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
    let getUserData (userId: int): HttpHandler =
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
        let rdate = System.DateTime(y, m, d)
        fun next ctx ->
            query { 
                for record in dataCtx.Public.Calories do
                    where (record.UserId = userId && record.ConsumeDate = rdate)
                    select (Mappings.userData record)
            } |> Seq.toList |> function
            | [] ->   RequestErrors.NOT_FOUND "Not Found" next ctx
            | data -> ctx.WriteJsonAsync data
            
open Implementation
            
let handler : HttpHandler =
    choose [
        route "/users" >=> GET >=> users
        routef "/users/%i" (fun u -> GET >=> getUser u)
        GET >=> (routef "/users/%i/data" getUserData)
        GET >=> (routef "/users/%i/data/%i-%i-%i" getUserDataAt)
    ]
