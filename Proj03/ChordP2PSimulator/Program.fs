open System
open System.Text
open System.Numerics
open System.Threading
open System.Globalization
open System.Collections.Generic

open Akka
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
                                    seed-nodes = [""akka.tcp://ChordP2PSimulator@localhost:9090""]
                                }
                            }
                        ")

    let sys = System.create "ChordP2PSimulator" configuration
    let mediator = DistributedPubSub.Get(sys).Mediator
    let printer = sys.ActorOf(Props(typeof<PrinterActor>), "printer")
    let chordManager = sys.ActorOf(Props(typeof<ChordManagerActor>, [| numberOfNodes :> obj; numberOfEachNodeRequests :> obj |]), "chordManager")
    // chordManager <! new CheckChordStructure()

    let testCases = [| "25_184_151_96_40780"; "94_244_58_243_37488"; "113_46_96_93_51209";
                       "79_226_228_148_8865"; "121_19_65_82_9338"; "7_51_219_205_53593";
                    |]

    let entries = new SortedDictionary<string, string>()
    let nodes = new List<string>()
    for i in 1 .. numberOfNodes do
        let identifier =  ToolsKit.generateNodeIdentifier() //testCases.[i - 1] 
        nodes.Add(identifier) |> ignore
        ToolsKit.addRecord(identifier)
        entries.Add(ToolsKit.encodeBySHA1(identifier), identifier)

    for entry in entries do
        mediator <! new Send("/user/printer", entry.Key + ":" + entry.Value, true)


    let nodesForRouter = nodes |> fun nodes -> 
                                    let nodePaths = new List<string>()
                                    for identifier in nodes do
                                        nodePaths.Add("/user/" + identifier)
                                    nodePaths
    let nodesBroadcastRouter = sys.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(nodesForRouter)), "nodesRouter")
    mediator <! new Put(nodesBroadcastRouter)

    let mutable firstNode = null
    let mutable firstNodeIdentifier = ""
    for i in 0 .. nodes.Count - 1 do
        let identifier = nodes.[i]
        printer <! "create [" + (i + 1).ToString() + "] " + identifier
        let newNode = sys.ActorOf(Props(typeof<ChordNodeActor>, [| identifier :> obj; numberOfEachNodeRequests :> obj |]), identifier)
        if firstNode = null then
            firstNode <- newNode
            firstNodeIdentifier <- identifier
        else
            if i = 1 then
                firstNode <! new UpdSuccessor(identifier)
                newNode <! new UpdSuccessor(firstNodeIdentifier)
            else 
                newNode <! new Join(firstNodeIdentifier)
            
        sys
            .Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromSeconds(0.0),
                TimeSpan.FromSeconds(0.5),
                newNode,
                new Stabilize(),
                ActorRefs.NoSender
            )

        sys
            .Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromSeconds(0.0),
                TimeSpan.FromSeconds(0.5),
                newNode,
                new FixFingerTable(),
                ActorRefs.NoSender
            )

    let mutable flg = false
    let printStructure() = async {
                                while flg = false do
                                    nodesBroadcastRouter <! new PrintContextInfo()
                                    do! Async.Sleep(1000)
                            } |> Async.StartAsTask |> ignore
    printStructure()
    async { do! Async.Sleep(2000) } |> Async.RunSynchronously
    chordManager <! new CheckChordStructure()

    let checkIsCompleteStructure() = 
        async {
            while ToolsKit.isBuiltStructure() = false do
                do! Async.Sleep(500)
                chordManager <! new CheckChordStructure()
            flg <- true
            printer <! "structure complete"
        } 

    async { do! Async.Sleep(2000) } |> Async.RunSynchronously
    checkIsCompleteStructure() |> Async.RunSynchronously
    flg <- true

    printer <! "start mission"
    // nodesBroadcastRouter <! new StartMission()

    Console.Read() |> ignore
    0