module DataAccess

open FSharp.Data.Sql

let [<Literal>] resolutionPath = __SOURCE_DIRECTORY__ + "\\..\\..\\..\\packages\\npgsql\\Npgsql\\lib\\netstandard2.0" 
let [<Literal>] connString = "Host=localhost;Database=expenses;Username=postgres;Password=sasa"

// create a type alias with the connection string and database vendor settings
type PostgreSqlExpenses =
    SqlDataProvider<
        DatabaseVendor = Common.DatabaseProviderTypes.POSTGRESQL,
        ConnectionString = connString,
        ResolutionPath = resolutionPath,
        UseOptionTypes = true,
        Owner = "public">

let dataCtx = PostgreSqlExpenses.GetDataContext()

let initializeDb () =
        
    printfn "Checking database initialization is required"
    let isEmptyDatabase = not <| query { for _ in dataCtx.Public.Users do exists(true) }

    if isEmptyDatabase then

        let tempPass = string (1000 + System.Random().Next 1000)

        let record = dataCtx.Public.Users.Create()
        // creating admin/admin user
        record.Login <- "admin"
        record.Name <- "root"
        record.Role <- Some "admin"
        record.TargetExpenses <- Some (decimal 0)
        record.Pwdhash <- CryptoHelpers.calculateHash tempPass

        dataCtx.SubmitUpdates()

        printfn "New database is initialized, please use admin/%s to connect to database" tempPass
