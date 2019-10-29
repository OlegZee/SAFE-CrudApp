module App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.DataProtection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Handlers

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../client/deploy"
let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let webApp =
    choose [
        subRoute "/api/v1" ApiV1.handler
        Auth.handler
        setStatusCode 404 >=> text "Not Found" ]

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()    // .UseHttpsRedirection()   FIXME
        .UseStaticFiles()
        .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services
        .AddGiraffe()
        .AddDataProtection().PersistKeysToFileSystem(DirectoryInfo @"APP_DATA")
        |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =

    DataAccess.initializeDb()
    
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(publicPath)
        // .UseIISIntegration()
        .UseWebRoot(publicPath)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
        .Build()
        .Run()
    0
