open System
open System.Collections.Generic

open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

open ToolsKit
open Msgs
open PrinterActor
open ChordManagerActor
open ChordNodeActor

[<EntryPoint>]
let main argv =

    let numberOfNodes = argv.[0] |> int
    let numberOfEachNodeRequests = argv.[1] |> int

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
                                    seed-nodes = [""akka.tcp://ChordP2PSystemSimulator@localhost:9090""]
                                }
                            }
                        ")

    let sys = System.create "ChordP2PSystemSimulator" configuration
    let mediator = DistributedPubSub.Get(sys).Mediator
    let printer = sys.ActorOf(Props(typeof<PrinterActor>), "printer")
    let chordManager = sys.ActorOf(Props(typeof<ChordManagerActor>, [| numberOfNodes :> obj; numberOfEachNodeRequests :> obj |]), "chordManager")

    let mutable firstNode = null
    let mutable firstNodeIdentifier = null
    let nodes = new List<string>()
    for i in 1 .. numberOfNodes do
        let newIdentifier = ToolsKit.generateNodeIdentifier()
        ToolsKit.addNodeIdentifierEntry(newIdentifier)
        nodes.Add("/user/" + newIdentifier)
        let newNode = sys.ActorOf(Props(typeof<ChordNodeActor>, [| newIdentifier :> obj; numberOfEachNodeRequests :> obj |]), newIdentifier)
        if firstNode = null then
            firstNode <- newNode
            firstNodeIdentifier <- newIdentifier
            printer <! "first identifier: " + firstNodeIdentifier
        else
            newNode <! new Join(firstNodeIdentifier)
        newNode <! new Stabilize()
        newNode <! new FixFingerTable()

    let nodesIdentifierInfo = ToolsKit.getNodeIndentifiers()
    for entry in nodesIdentifierInfo do
        printer <! entry.Key + ":" + entry.Value

    let nodesBroadcastRouter = sys.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(nodes)), "nodesRouter")
    mediator <! new Put(nodesBroadcastRouter)

    let mutable printingFlg = true
    async {
        while printingFlg do
            nodesBroadcastRouter <! new PrintContextInfo()
            do! Async.Sleep(1000)
    } |> Async.StartAsTask |> ignore

    async {
        chordManager <! new CheckChordStructure()
        while ToolsKit.checkCompleteStructure() = false do
            chordManager <! new CheckChordStructure()
            do! Async.Sleep(500)
        printingFlg <- false
    } |> Async.RunSynchronously |> ignore

    printer <! "structure complete"
    printer <! "start mission"
    nodesBroadcastRouter <! new StopStabilize()

    nodesBroadcastRouter <! new StartLookupMission()

    Console.Read() |> ignore
    0 // return an integer exit code