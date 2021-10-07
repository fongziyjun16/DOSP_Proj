open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

open Msgs
open Actors

let systemName = "PSLSystem"
let numberOfWorkers = 10000

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
                                    seed-nodes = [""akka.tcp://PSLSystem@localhost:9090""]
                                }
                            }
                        ")

    let PSLSystem = System.create systemName (configuration)

    let recorder = PSLSystem.ActorOf(Props(typeof<RecorderActor>, [| numberOfWorkers :> obj |]), "recorder")
    
    PSLSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore
    
    let workerList = new Collections.Generic.List<string>()

    for i in 1 .. numberOfWorkers do
        let name = "worker_" + i.ToString()
        PSLSystem.ActorOf(Props(typeof<PSLWorkerActor>, [| i :> obj; numberOfWorkers :> obj |]), name) |> ignore
        workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(PSLSystem).Mediator

    let broadcastRouter = PSLSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    mediator <! new Put(broadcastRouter)

    let randomRouter = PSLSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    mediator <! new Put(randomRouter)

    recorder <! new Start()

    Console.Read() |> ignore
    0 // return an integer exit code