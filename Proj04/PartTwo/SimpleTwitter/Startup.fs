namespace SimpleTwitter

open System.IO
open System.Data.SQLite
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open WebSharper.AspNetCore
open WebSharper.AspNetCore.WebSocket

type Startup() =

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddSitelet(Site.Main)
            .AddAuthentication("WebSharper")
            .AddCookie("WebSharper", fun options -> ())
        |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore

        app.UseAuthentication()
            .UseStaticFiles()
            .UseWebSockets()
            .UseWebSharper(
                fun builder ->
                    builder.UseWebSocket(
                        "ws",
                        fun wsBuilder ->
                            wsBuilder
                                .Use(RealTimeServer.Start())
                                .JsonEncoding(JsonEncoding.Readable) |> ignore
                    )
            )
            .Run(fun context ->
                context.Response.StatusCode <- 404
                context.Response.WriteAsync("Page not found"))

module Program =
    
    let BuildDB() =
        let dbFile = @"./resources/twitter_sys.db"
        if File.Exists(dbFile) then
            File.Delete(dbFile)

        let createTablesScript = File.ReadAllText(@"./resources/create_table.sql")

        SQLiteConnection.CreateFile(@"./resources/tweet_sys.db")
        use sqliteConnection = new SQLiteConnection(@"Data Source=./resources/twitter_sys.db")
        sqliteConnection.Open()
        let command = new SQLiteCommand(createTablesScript, sqliteConnection)
        command.ExecuteNonQuery() |> ignore
        sqliteConnection.Close()

    
    let BuildWebHost args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()

    [<EntryPoint>]
    let main args =
        BuildDB()
        BuildWebHost(args).Run()
        0
