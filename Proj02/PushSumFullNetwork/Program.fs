open Akka.FSharp

open Akka.Actor
open Akka.FSharp
open Akka.Cluster
open Akka.Configuration

open Msgs
open Actors

let numberOfWorkers = 8
let sysName = "PushSumFullNetworkSystem"

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
                                    seed-nodes = [""akka.tcp://PushSumFullNetworkSystem@localhost:9090""]
                                }
                            }
                        ")
    
    let PushSumFullNetworkSystem = System.create sysName (configuration)

    let recorder = PushSumFullNetworkSystem.ActorOf(Props(typeof<RecorderActor>, [| numberOfWorkers :> obj |]), "recorder")

    for i in 1 .. numberOfWorkers do
        PushSumFullNetworkSystem.ActorOf(Props(typeof<PushSumFullNetworkWorkerActor>, [| i :> obj; numberOfWorkers :> obj |]), "worker_" + i.ToString()) |> ignore
    
    recorder <! (new StartRumor())

    System.Console.Read() |> ignore
    0 // return an integer exit code