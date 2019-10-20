module DataAccess.SqlModel

open FSharp.Data.Sql

let [<Literal>] resolutionPath = __SOURCE_DIRECTORY__ + "\\..\\..\\..\\packages\\npgsql\\Npgsql\\lib\\netstandard2.0" 
let [<Literal>] connString = "Host=localhost;Database=calories;Username=postgres;Password=sasa"

// create a type alias with the connection string and database vendor settings
type sql =
    SqlDataProvider<
        DatabaseVendor = Common.DatabaseProviderTypes.POSTGRESQL,
        ConnectionString = connString,
        ResolutionPath = resolutionPath,
        UseOptionTypes = true,
        Owner = "public">

let ctx = sql.GetDataContext()
