open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

open Msgs
open Actors

let systemName = "PSFNSystem"
let numberOfWorkers = 100000

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
    
    let workerList = new Collections.Generic.List<string>()

    for i in 1 .. numberOfWorkers do
        let name = "worker_" + i.ToString()
        PSFNSystem.ActorOf(Props(typeof<PSFNWorkerActor>, [| i :> obj |]), name) |> ignore
        workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(PSFNSystem).Mediator

    let broadcastRouter = PSFNSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadCastRouter")
    mediator <! new Put(broadcastRouter)

    let randomRouter = PSFNSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    mediator <! new Put(randomRouter)

    recorder <! new Start()

    Console.Read() |> ignore
    0 // return an integer exit code