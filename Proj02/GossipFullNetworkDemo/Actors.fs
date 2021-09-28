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

    let Cluster = Akka.Cluster.Cluster.Get(Actor.Context.System)
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        Cluster.Subscribe(Actor.Context.Self, ClusterEvent.InitialStateAsEvents, [| typeof<ClusterEvent.IMemberEvent> |])
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

    override x.PostStop() =
        Cluster.Unsubscribe(Actor.Context.Self)

type WorkerActor(id: int, numberOfWorkers: int, rumorTimes: int) =
    inherit Actor()

    let addrs = new Collections.Generic.List<int>()
    let mutable rumorCounter = 0

    let Cluster = Akka.Cluster.Cluster.Get(Actor.Context.System)
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        Cluster.Subscribe(Actor.Context.Self, ClusterEvent.InitialStateAsEvents, [| typeof<ClusterEvent.IMemberEvent> |])
        mediator <! (new Put(Actor.Context.Self))
        for i in 1 .. numberOfWorkers do
            if i <> id then
                addrs.Add(i)

    override x.OnReceive message =
        match box message with
        | :? Rumor as msg ->
            if rumorCounter < rumorTimes then
                rumorCounter <- (rumorCounter + 1)
                let nextID = (addrs.[Random().Next(numberOfWorkers - 1)])
                mediator <! (new Send("/user/worker_" + nextID.ToString(), msg, true))
                mediator <! (new Send("/user/recorder", new GetRumor(), true))
        | :? ClusterEvent.IMemberEvent as msg -> ()
        | _ -> printfn "unknown message"

    override x.PostStop() =
        Cluster.Unsubscribe(Actor.Context.Self)
        