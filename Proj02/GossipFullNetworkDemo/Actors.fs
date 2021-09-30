module Actors

open System
open Akka.Actor
open Akka.FSharp
open Akka.Cluster
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()
    
    let mutable stopFlg = false
    let mutable stopTellingCounter = 0

    let mutable realTimeStart = new DateTime()
    let mutable realTimeEnd = new DateTime()
    let stopWatch = new Diagnostics.Stopwatch()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            realTimeStart <- DateTime.Now
            stopWatch.Start()
            mediator <! (new Send("/user/worker_" + (Random().Next(numberOfWorkers) + 1).ToString(), new Rumor(), true))
        | :? GetRumor as msg ->
            if stopTellingCounter <> numberOfWorkers then
                stopTellingCounter <- (stopTellingCounter + 1)
            else 
                if stopFlg = false then
                    stopFlg <- true
                    realTimeEnd <- DateTime.Now
                    stopWatch.Stop()
                    printfn "stop rumor. real time span: %A ; process run how long: %A" 
                            (realTimeEnd.Subtract(realTimeStart).Milliseconds) 
                            stopWatch.Elapsed.Milliseconds
        | :? ClusterEvent.IMemberEvent as msg -> ()
        | _ -> printfn "unknown message"


type FullNetworkWorkerActor(id: int, numberOfWorkers: int, rumorTimes: int) =
    inherit Actor()

    let mutable rumorCounter = 0

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))

    override x.OnReceive message =
        match box message with
        | :? Rumor as msg ->
            if rumorCounter < rumorTimes then
                rumorCounter <- (rumorCounter + 1)
                mediator <! (new Send("/user/worker_" + (x.GetRandomNeighbor()).ToString(), msg, true))
                mediator <! (new Send("/user/recorder", new GetRumor(), true))
        | _ -> printfn "unknown message"

    member x.GetRandomNeighbor() =
        let random = new Random()
        if id = 1 then 
            random.Next(2, numberOfWorkers + 1)
        else if id = numberOfWorkers then 
            random.Next(1, numberOfWorkers)
        else 
            let candidates = [| random.Next(1,id); random.Next(id + 1, numberOfWorkers) |]
            candidates.[random.Next(0, 2)]
        