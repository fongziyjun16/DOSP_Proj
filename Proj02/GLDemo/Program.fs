open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

open Msgs
open Actors

let numberOfWorkers = 1000
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

    GLSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore

    let workerList = new Collections.Generic.List<string>()

    for i in 1 .. numberOfWorkers do
        let name = "worker_" + i.ToString()
        GLSystem.ActorOf(Props(typeof<GLWorkerActor>, [| i :> obj; numberOfWorkers :> obj; rumorLimit :> obj |]), name) |> ignore
        workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(GLSystem).Mediator

    let broadcastRouter = GLSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    mediator <! new Put(broadcastRouter)

    let randomRouter = GLSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    mediator <! new Put(randomRouter)

    recorder <! new StartRumor()

    Console.Read() |> ignore
    0 // return an integer exit code