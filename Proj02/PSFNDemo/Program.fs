open System
open Akka.Actor
open Akka.FSharp
open Akka.Configuration

open Msgs
open Actors

let systemName = "PSFNSystem"
let numberOfWorkers = 100

[<EntryPoint>]
let main argv =

    let configuration = ConfigurationFactory.ParseString(@"
                            akka {
                                actor.provider = cluster
                                remote {
                                    dot-netty.tcp {
                                        port = 9090
                                        hostname = localhost
                                    }
                                }
                                cluster {
                                    seed-nodes = [""akka.tcp://PSFNSystem@localhost:9090""]
                                }
                            }
                        ")

    let PSFNSystem = System.create systemName (configuration)

    let recorder = PSFNSystem.ActorOf(Props(typeof<RecorderActor>, [| numberOfWorkers :> obj |]), "recorder")
    
    PSFNSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore
    
    for i in 1 .. numberOfWorkers do
        PSFNSystem.ActorOf(Props(typeof<PSFNWorkerActor>, [| i :> obj; numberOfWorkers :> obj |]), "worker_" + i.ToString()) |> ignore
    
    recorder <! new StartRumor()

    Console.Read() |> ignore
    0 // return an integer exit code