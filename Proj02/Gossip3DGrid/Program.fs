open System
open Akka.Actor
open Akka.FSharp
open Akka.Cluster
open Akka.Configuration

open Msgs
open Actors

let getRumorLimit = 10
let sysName = "Gossip3DGridSystem"
let lenght = 3
let width = 3
let height = 3

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
    
    let Gossip3DGridSystem = System.create sysName (configuration)

    let gridStructure = new GridStructure(lenght, width, height)

    let recorder = Gossip3DGridSystem.ActorOf(Props(typeof<RecorderActor>, [| gridStructure :> obj |]), "recorder")

    for i in 0 .. (lenght - 1) do
        for j in 0 .. (width - 1) do
            for k in 0 .. (height - 1) do
                let actorName = "worker_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString()
                Gossip3DGridSystem.ActorOf(Props(typeof<Gossip3DGridWorkerActor>, [| new WorkerPosition(i, j, k) :> obj; gridStructure :> obj; getRumorLimit :> obj |]), actorName) |> ignore

    recorder <! new StartRumor()

    System.Console.Read() |> ignore
    0 // return an integer exit code