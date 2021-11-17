open System

open Akka.Actor
open Akka.FSharp
open Akka.Configuration

open Actor

[<EntryPoint>]
let main argv =
    
    let numberOfClient = argv.[0] |> int
    
    let configuration = ConfigurationFactory.ParseString(@"
                            akka {
                                actor.provider = cluster
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
    let randomController = tweetSimulator.ActorOf(Props(typeof<RandomControllerActor>, [| numberOfClient :> obj |]), "randomController")
    


    Console.Read() |> ignore
    0 // return an integer exit code