module ChordNodeActor

open System
open System.Collections.Generic

open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Cluster.Tools.PublishSubscribe

open Tools
open Msgs

type ChordNodeActor(numberOfRequest: int, ip: string) =
    inherit Actor()

    let mutable predecessor = ""
    let mutable successor = ""
    let mutable requestCounter = 0

    let resources = new HashSet<string>()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message = 
        match box message with
        | :? ContextInfo as msg ->
            predecessor <- msg.PREDECESSOR
            successor <- msg.SUCCESSOR
        | :? SetPredecessor as msg ->
            predecessor <- msg.PREDECESSOR
        | :? SetSuccessor as msg ->
            successor <- msg.SUCCESSOR
        | :? AddNewResource as msg ->
            resources.Add(msg.NAME) |> ignore
        | :? FindingRequest as msg ->
            printfn "HaHa"
        | :? StartFindResource as msg ->
            if resources.Contains(msg.NAME) then
                Actor.Context.Self <! new GetResource(msg.NAME, ip, 0)
            else
                let checkMsg = new CheckResource(msg.NAME, ip)
                checkMsg.IncrTimes()
                mediator <! new Send("/user/" + buildChordNodeName(successor), checkMsg, true)
        | :? CheckResource as msg ->
            let isContained = resources.Contains(msg.GetName())
            if isContained then
                mediator <! new Send("/user/" + buildChordNodeName(msg.GetFrom()), 
                                    new GetResource(msg.GetName(), ip, msg.GetTimes()), true)
            else
                msg.IncrTimes()
                mediator <! new Send("/user/" + buildChordNodeName(successor), msg, true)
        | :? GetResource as msg ->
            mediator <! new Send("/user/coordinator", new ReportTimes(msg.TIMES, ip), true)
        | _ -> printfn "%s get unknown message" Actor.Context.Self.Path.Name




