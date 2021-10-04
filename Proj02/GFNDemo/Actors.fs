module Actors

open System
open Akka.Actor
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()

    let mutable round = 0
    let mutable rumorNO = 0
    let mutable numberOfGetRumor = 0

    let mutable realTimeStart = DateTime.Now
    let mutable realTimeEnd = DateTime.Now
    let stopWatch = new Diagnostics.Stopwatch()

    let eventManager = Actor.Context.System.EventStream

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message = 
        match box message with
        | :? StartRumor as msg ->
            stopWatch.Start()
            x.NewRoundDissemination()
        | :? EndRumor as msg ->
            if numberOfGetRumor < numberOfWorkers then
                numberOfGetRumor <- (numberOfGetRumor + 1)
                if numberOfGetRumor = numberOfWorkers then
                    eventManager.Publish(new AllGetRumor())
                    realTimeEnd <- DateTime.Now
                    stopWatch.Stop()
                    printfn "total rounds: %d, rumorNO: %d" round rumorNO
                    let realTime = realTimeEnd.Subtract(realTimeStart)
                    printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (realTime.Minutes) (realTime.Seconds) (realTime.Milliseconds)
                    let runTime = stopWatch.Elapsed
                    printfn "run time -- minutes: %d seconds: %d milliseconds: %d" (runTime.Minutes) (runTime.Seconds) (runTime.Milliseconds)
        | :? ReqNewRoundDissemination as msg ->
            if numberOfGetRumor < numberOfWorkers && msg.NO = rumorNO then
                x.NewRoundDissemination()
        | :? ReqNewRumorNO as msg ->
            Actor.Context.Sender <! x.GetRumorNOWithIncr()
        | :? NumberOfActorGetRumor as msg ->
            Actor.Context.Sender <! numberOfGetRumor
        | _ -> printfn "unkown message"

    member x.GetRumorNOWithIncr() =
        rumorNO <- (rumorNO + 1)
        rumorNO
    
    member x.NewRoundDissemination() =
        round <- (round + 1)
        mediator <! new Send("/user/worker_" + (Random().Next(numberOfWorkers) + 1).ToString(), new Rumor(x.GetRumorNOWithIncr()), true)

type TaskProcessorActor(id: int, numberOfWorkers: int, rumorLimit: int) =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message = 
        match box message with
        | :? Rumor as msg ->
            while Async.RunSynchronously(Actor.Context.Parent <? new SingleActorStopSendingFlg(), -1) = false do
                msg.SetRumorNO(Async.RunSynchronously(mediator <? new Send("/user/recorder", new ReqNewRumorNO(), true), -1))
                mediator <! new Send("/user/worker_" + x.GetRandomNeighbor().ToString(), msg, true)
            // printfn "stop"
        | _ -> printfn "unkown message"

    member x.GetRandomNeighbor() =
        let random = new Random()
        if id = 1 then 
            random.Next(2, numberOfWorkers + 1)
        else if id = numberOfWorkers then 
            random.Next(1, numberOfWorkers)
        else 
            let candidates = [| random.Next(1,id); random.Next(id + 1, numberOfWorkers) |]
            candidates.[random.Next(0, 2)]

type GFNWorkerActor(id: int, numberOfWorkers: int, rumorLimit: int) =
    inherit Actor()

    let mutable stopSendingFlg = false
    let mutable getRumorFlg = false
    let mutable numberOfGetRumor = 0;

    let mutable taskProcessorWorkingFlg = false
    let taskProcessor = Actor.Context.ActorOf(Props(typeof<TaskProcessorActor>, [| id :> obj; numberOfWorkers :> obj; rumorLimit :> obj |]), "taskProcessor")
    
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message = 
        match box message with
        | :? Rumor as msg ->
            if numberOfGetRumor < rumorLimit then
                numberOfGetRumor <- (numberOfGetRumor + 1)
                if getRumorFlg = false then
                    getRumorFlg <- true
                    mediator <! new Send("/user/recorder", new EndRumor(), true)
                if taskProcessorWorkingFlg = false then
                    taskProcessorWorkingFlg <- true
                    taskProcessor <! msg
            else
                mediator <! new Send("/user/recorder", new ReqNewRoundDissemination(msg.NO), true)
        | :? SingleActorStopSendingFlg as msg ->
            Actor.Context.Sender <! stopSendingFlg
        | :? AllGetRumor as msg ->
            stopSendingFlg <- true
        | _ -> printfn "unkown message"


