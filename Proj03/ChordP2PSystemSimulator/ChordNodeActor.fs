module ChordNodeActor

open System

open Akka.Actor
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open ToolsKit
open Msgs
open ChordNodeInfo
open ChordNodeAssitantActor

type ChordNodeActor(identifier: string, numberOfRequests: int) =
    inherit Actor()

    let context = Actor.Context
    let nodeInfo = new ChordNodeInfo(identifier)
    let mutable assitant = null

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let print(info: string) =
        mediator <! new Send("/user/printer", info, true)

    override this.PreStart() =
        nodeInfo.initialize()
        mediator <! new Put(Actor.Context.Self)
        assitant <- context.ActorOf(Props(typeof<ChordNodeAssitantActor>, [| nodeInfo :> obj |]), "assistant")

    override this.OnReceive message =
        match box message with
        | :? AskPredecessor as msg ->
            this.Sender <! nodeInfo.getPredecessor()
        | :? AskContextInfo as msg ->
            this.Sender <! nodeInfo.getPredecessor() + ":" + nodeInfo.getSuccessor()
        | :? PrintContextInfo as msg ->
            let printingInfo = nodeInfo.getPredecessor() + ":" + nodeInfo.getSuccessor()
            print(printingInfo)
        | :? Join as msg ->
            let target = context.ActorSelection(context.Self.Path.Root.ToString() + "/user/" + msg.ID)
            let findSuccessor() = 
                (string) (Async.RunSynchronously(target <? new FindSuccessor(nodeInfo.getIdentifier(), false), -1))
            let mutable newSuccessor = findSuccessor()
            while newSuccessor.IndexOf("true") = -1 do
                newSuccessor <- findSuccessor()
            nodeInfo.setSuccessor(newSuccessor.Split(':').[0])
        | :? FindSuccessor as msg ->
            let key = msg.KEY
            let mutable keyCode = ""
            if msg.ENCODED then keyCode <- key
            else keyCode <- ToolsKit.encodeBySHA1(key)

            let mutable nextNode = ""

            if nodeInfo.isInSuccessor(keyCode) then
                nextNode <- nodeInfo.getSuccessor() + ":true"
            else
                nextNode <- nodeInfo.findInFigerTable(keyCode)
                if nextNode.Equals(nodeInfo.getIdentifier()) then
                    nextNode <- nodeInfo.getIdentifier() + ":true"
                else 
                    let parts = nextNode.Split(':')
                    nextNode <- parts.[0]
                    let target = context.ActorSelection(context.Self.Path.Root.ToString() + "/user/" + nextNode)
                    try
                        let task = target.Ask(msg, TimeSpan.FromSeconds(3.0))
                        nextNode <- (string) (Async.RunSynchronously(task, -1))
                    with
                    | :? AggregateException ->
                        nextNode <- ":false"
                        // print("find ( "+ key + ") successor time out")
            this.Sender <! nextNode
        | :? Stabilize as msg ->
            context.System
                .Scheduler.ScheduleTellRepeatedly(
                    TimeSpan.FromSeconds(0.0),
                    TimeSpan.FromSeconds(0.2),
                    assitant,
                    msg,
                    ActorRefs.NoSender
                )
        | :? Notify as msg ->
            let other = msg.ID
            let otherCode = ToolsKit.toBigInteger(other)
            if nodeInfo.getPredecessor().Equals("") || 
                ToolsKit.isInScope(otherCode, nodeInfo.getPredecessorCode(), 
                                   nodeInfo.getSuccessorCode(), true, true) then
                nodeInfo.setPredecessor(other)
        | :? StopStabilize as msg ->
            nodeInfo.hasStabilized()
        | :? StopFixFingerTable as msg ->
            nodeInfo.stopFixFingerTable()
        | :? FixFingerTable as msg ->
            context.System
                .Scheduler.ScheduleTellRepeatedly(
                    TimeSpan.FromSeconds(0.0),
                    TimeSpan.FromSeconds(0.5),
                    assitant,
                    msg,
                    ActorRefs.NoSender
                )
        
        // lookup processing part
        | :? StartLookupMission as msg ->
            let mission = context
                            .System
                            .Scheduler
                            .ScheduleTellRepeatedlyCancelable(
                                TimeSpan.FromSeconds(0.0),
                                TimeSpan.FromSeconds(1.0),
                                context.Self,
                                new PreLookup(),
                                ActorRefs.NoSender
                            )
            mission.CancelAfter(numberOfRequests * 1000)
        | :? PreLookup as msg ->
            let resource = ToolsKit.generateOneRandomResource()
            let lookupInfo = new Lookup(resource, nodeInfo.getIdentifier())
            context.Self <! lookupInfo
        | :? Lookup as msg ->
            let key = msg.getKey()
            let keyCode = ToolsKit.encodeBySHA1(key)
            if nodeInfo.isInSuccessor(keyCode) then
                mediator <! new Send("/user/chordManager", new FoundResource(msg.getKey(), msg.getSteps(), msg.getPublisher()), true)
            else
                let nextNode = nodeInfo.findInFigerTable(keyCode)
                if nextNode.Equals(nodeInfo.getIdentifier) then
                    mediator <! new Send("/user/chordManager", new FoundResource(msg.getKey(), msg.getSteps(), msg.getPublisher()), true)
                else
                    msg.incrSteps()
                    mediator <! new Send("/user/" + nextNode, msg, true)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name
