open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

open Msgs
open Actors

let length = 3
let width = 5
let height = 4
let numberOfWorkers = 10
let rumorLimit = 10
let systemName = "GI3DGSystem"


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
                                    seed-nodes = [""akka.tcp://GI3DGSystem@localhost:9090""]
                                }
                            }
                        ")
    
    let GI3DGSystem = System.create systemName (configuration)

    let structure = new Structure(length, width, height)

    let recorder = GI3DGSystem.ActorOf(Props(typeof<RecorderActor>, [| structure :> obj |]), "recorder")

    GI3DGSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore

    let workerList = new Collections.Generic.List<string>()

    for i in 1 .. length do
        for j in 1 .. width do
            for k in 1 .. height do
                let position = new Position(i, j, k)
                let name = "worker_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString()
                GI3DGSystem.ActorOf(Props(typeof<GFNWorkerActor>, [| position :> obj; structure :> obj; rumorLimit :> obj |]), name) |> ignore
                workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(GI3DGSystem).Mediator

    let broadcastRouter = GI3DGSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    mediator <! new Put(broadcastRouter)

    let randomRouter = GI3DGSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    mediator <! new Put(randomRouter)

    recorder <! new StartRumor()

    Console.Read() |> ignore
    0 // return an integer exit code