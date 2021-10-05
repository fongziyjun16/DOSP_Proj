open System
open Akka.Actor
open Akka.FSharp
open Akka.Configuration

open Msgs
open Actors

let numberOfWorkers = 100
let rumorLimit = 10
let systemName = "GLSystem"

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
                                    seed-nodes = [""akka.tcp://GLSystem@localhost:9090""]
                                }
                            }
                        ")
    
    let GLSystem = System.create systemName (configuration)


    let recorder = GLSystem.ActorOf(Props(typeof<RecorderActor>, [| numberOfWorkers :> obj |]), "recorder")

    let printer = GLSystem.ActorOf(Props(typeof<PrinterActor>), "printer")

    for i in 1 .. numberOfWorkers do
        GLSystem.ActorOf(Props(typeof<GLWorkerActor>, [| i :> obj; numberOfWorkers :> obj; rumorLimit :> obj |]), "worker_" + i.ToString()) |> ignore

    recorder <! new StartRumor()

    Console.Read() |> ignore
    0 // return an integer exit code