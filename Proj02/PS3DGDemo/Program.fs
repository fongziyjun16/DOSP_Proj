open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

open Msgs
open Actors

let length = 5
let width = 5
let height = 5
let systemName = "PS3DSystem"

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
                                    seed-nodes = [""akka.tcp://PS3DSystem@localhost:9090""]
                                }
                            }
                        ")

    let PS3DSystem = System.create systemName (configuration)

    let structure = new Structure(length, width, height)

    let recorder = PS3DSystem.ActorOf(Props(typeof<RecorderActor>, [| structure :> obj |]), "recorder")
    
    PS3DSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore
    
    let workerList = new Collections.Generic.List<string>()

    for i in 1 .. length do
        for j in 1 .. width do
            for k in 1 .. height do
                let position = new Position(i, j, k)
                let name = "worker_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString()
                PS3DSystem.ActorOf(Props(typeof<PS3DWorkerActor>, [| position :> obj; structure :> obj; rumorLimit :> obj |]), name) |> ignore
                workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(PS3DSystem).Mediator

    let broadcastRouter = PS3DSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    mediator <! new Put(broadcastRouter)

    let randomRouter = PS3DSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    mediator <! new Put(randomRouter)

    recorder <! new Start()

    Console.Read() |> ignore
    0 // return an integer exit code