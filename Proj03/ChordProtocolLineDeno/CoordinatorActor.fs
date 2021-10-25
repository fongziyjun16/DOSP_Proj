module CoordinatorActor

open System
open System.Collections.Generic

open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Cluster.Tools.PublishSubscribe

open Tools
open Msgs

type CoordinatorActor(numberOfChordNodes: int, numberOfEachNodeRequest: int) =
    inherit Actor()

    let mutable requestCounter = (double)0
    let mutable timesCounter = 0
    let totalNubmerOfRequest = numberOfChordNodes * numberOfEachNodeRequest
    let table = new SortedDictionary<string, string>()
    let mutable keys = table.Keys |> List<string>

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message = 
        match box message with
        | :? AddNewChordNode as msg ->
            table.Add(msg.ENCODE, msg.IP)
            keys <- table.Keys |> List<string>

            let predecessorIP = table.[this.findPredecessor(msg.ENCODE)]
            let successorIP = table.[this.findSuccessor(msg.ENCODE)]
            let chordNodeContextInfo = new ContextInfo(
                                        predecessorIP, 
                                        successorIP
                                        )

            mediator <! new Send("/user/" + buildChordNodeName(predecessorIP), new SetPredecessor(msg.IP), true)
            mediator <! new Send("/user/" + buildChordNodeName(msg.IP), chordNodeContextInfo, true)
            mediator <! new Send("/user/" + buildChordNodeName(successorIP), new SetSuccessor(msg.IP), true)
        | :? AssignNewResource as msg ->
            let key = getSHA1(msg.NAME)
            let successor = this.findSuccessor(key)

            mediator <! new Send("/user/" + buildChordNodeName(table.[successor]), new AddNewResource(msg.NAME), true)
        | :? ReportTimes as msg ->
            requestCounter <- requestCounter + 1.0
            timesCounter <- timesCounter + msg.TIMES
            if requestCounter = (double) totalNubmerOfRequest then
                mediator <! new Send("/user/printer", new PrintAvgHops((double) timesCounter / requestCounter))
        | _ -> printfn "%s get unknown message" Actor.Context.Self.Path.Name

    member this.findPredecessor(key: string): string =
        if key.CompareTo(keys.[0]) < 0 || key.CompareTo(keys.[keys.Count - 1]) > 0 then
            keys.[keys.Count - 1]
        else this.binarySearch(key, false)
    
    member this.findSuccessor(key: string): string = 
        if key.CompareTo(keys.[0]) < 0 || key.CompareTo(keys.[keys.Count - 1]) > 0 then
            keys.[0]
        else this.binarySearch(key, true)

    member this.binarySearch(key:string, dir: bool): string = // dir false -- left ; true -- right
        let mutable left = 0
        let mutable right = keys.Count - 1
    
        while left < right - 1 do
            let mid = left + (right - left) / 2
            if keys.[mid].CompareTo(key) > 0 then
                right <- mid
            else
                left <- mid
        
        if dir then // true right
            keys.[right]
        else // false left
            keys.[left]




