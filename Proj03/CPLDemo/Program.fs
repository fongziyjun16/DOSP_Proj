open System

open Akka.Actor
open Akka.FSharp
open Akka.Configuration

open Tools
open Msgs
open PrinterActor
open ResourcesManagerActor
open ChordManagerActor
open ChordNodeActor

[<EntryPoint>]
let main argv =
    
    let nubmerOfNodes = argv.[0] |> int
    let nubmerOfEachNodeRequests = argv.[1] |> int

    let randomIPs = getRandomIPs(nubmerOfNodes)
    let numberOfResources = nubmerOfNodes * nubmerOfEachNodeRequests

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
                                    seed-nodes = [""akka.tcp://CPLDemo@localhost:9090""]
                                }
                            }
                        ")

    let sys = System.create "CPLDemo" (configuration)

    let printer = sys.ActorOf(Props(typeof<PrinterActor>), "printer") 

    let chordManager = sys.ActorOf(Props(typeof<ChordManagerActor>, [| numberOfResources :> obj |]), "chordManager")

    for ip in randomIPs do
        sys.ActorOf(Props(typeof<ChordNodeActor>, [| ip :> obj; nubmerOfEachNodeRequests :> obj |]), buildNodeNameByIP(ip)) |> ignore
        chordManager <! new AddNewChordNode(getSHA1(ip), ip)

    Async.RunSynchronously(chordManager <? new NotifyNodeContext(), -1) |> ignore

    let resourcesManager = sys.ActorOf(Props(typeof<ResourcesManagerActor>, [| numberOfResources :> obj |]), "resourcesManager")

    Async.RunSynchronously(resourcesManager <? new AssignAllResources(), -1) |> ignore

    chordManager <! new NotifyNodeRequest()

    Console.Read() |> ignore
    0 // return an integer exit code