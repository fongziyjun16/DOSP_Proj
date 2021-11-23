open System
open System.IO
open System.Data.SQLite

open Akka.Actor
open Akka.FSharp
open Akka.Configuration

open Msgs
open Actor

[<EntryPoint>]
let main argv =
    
    let numberOfClients = argv.[0] |> int
    
    let dbFile = @"./resources/tweet_sys.db"
    if File.Exists(dbFile) then
        File.Delete(dbFile)

    let createTablesScript = File.ReadAllText(@"./resources/create_tables.sql")

    SQLiteConnection.CreateFile(@"./resources/tweet_sys.db")
    use sqliteConnection = new SQLiteConnection(@"Data Source=./resources/tweet_sys.db")
    sqliteConnection.Open()
    let command = new SQLiteCommand(createTablesScript, sqliteConnection)
    command.ExecuteNonQuery() |> ignore
    sqliteConnection.Close()

    // Tweet System Simulator using AKKA
    let configuration = ConfigurationFactory.ParseString(@"
                            akka {
                                actor.provider = remote
                                remote {
                                    dot-netty.tcp {
                                        port = 10012
                                        hostname = localhost
                                    }
                                }
                            }
                        ")

    let tweetSimulator = System.create "TweetSimulator" (configuration)

    let printer = tweetSimulator.ActorOf(Props(typeof<PrinterActor>), "printer")
    let tweetEngine = tweetSimulator.ActorOf(Props(typeof<TweetEngineActor>), "tweetEngine")
    let randomController = tweetSimulator.ActorOf(Props(typeof<RandomControllerActor>, [| numberOfClients :> obj |]), "randomController")
    
    randomController <! new RegisterCall()
    // randomController <! new LoginLogoutTest()
    randomController <! new CLientPostTest()

    Console.Read() |> ignore
    0 // return an integer exit code