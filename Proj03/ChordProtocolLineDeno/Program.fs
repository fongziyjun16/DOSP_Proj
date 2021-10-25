open System

open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration

open Tools
open Msgs
open PrinterActor
open CoordinatorActor
open ChordNodeActor

[<EntryPoint>]
let main argv =

    let numberOfNodes = argv.[0] |> int
    let numberOfRequest = argv.[1] |> int

    let randomIPs = getRandomIPs(numberOfNodes)
    let randomResources = getRandomResource(numberOfNodes * numberOfRequest)

    let configuration = ConfigurationFactory.ParseString(@"
                            akka {
                                actor.provider = remote
                                remote {
                                    dot-netty.tcp {
                                        port = 9090
                                        hostname = localhost
                                    }
                                }
                            }
                        ")

    let CPLSystem = System.create "CPLSystem" (configuration)

    CPLSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore
    let coordinator = CPLSystem.ActorOf(Props(typeof<CoordinatorActor>, [| numberOfNodes :> obj; numberOfRequest :> obj; |]), "coordinator")
    for i in 0 .. numberOfNodes - 1 do
        let ip = randomIPs.[i]
        CPLSystem.ActorOf(Props(typeof<ChordNodeActor>, [| numberOfRequest :> obj; ip :> obj; |]), buildChordNodeName(ip)) |> ignore
        coordinator <! new AddNewChordNode(ip, getSHA1(ip))
    
    for i in 0 .. randomResources.Count - 1 do
        coordinator <! new AssignNewResource(randomResources.[i])

    

    Console.Read() |> ignore
    0 // return an integer exit code