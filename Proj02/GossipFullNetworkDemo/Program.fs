open Akka.Actor
open Akka.FSharp
open Akka.Remote
open Akka.Cluster
open Akka.Configuration

open Msgs
open Actors

let numberOfWorkers = 1_000_000
let times = 41
let systemName = "GossipFullNetworkSystem"

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
                                    seed-nodes = [""akka.tcp://GossipFullNetworkSystem@localhost:9090""]
                                }
                            }
                        ")
    
    let GossipFullNetworkSystem = System.create systemName (configuration)

    let recorder = GossipFullNetworkSystem.ActorOf(Props(typeof<RecorderActor>, [| numberOfWorkers :> obj |]), "recorder")

    for i in 1 .. numberOfWorkers do
        GossipFullNetworkSystem.ActorOf(Props(typeof<FullNetworkWorkerActor>, [| i :> obj; numberOfWorkers :> obj; times :> obj |]), "worker_" + i.ToString()) |> ignore
    
    recorder <! (new StartRumor())

    System.Console.Read() |> ignore
    0