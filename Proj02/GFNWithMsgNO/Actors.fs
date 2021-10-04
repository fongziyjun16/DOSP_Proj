module Actors

open System
open Akka.Actor
open Akka.FSharp
open Akka.Cluster
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()
    
    let mutable rumorMsgNO = 0
    let mutable round = 0

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
            x.MotivateRumor()
        | :? GetRumor as msg ->
            if stopFlg = false then
                if stopTellingCounter < numberOfWorkers then
                    stopTellingCounter <- (stopTellingCounter + 1)
                    if stopTellingCounter = numberOfWorkers then
                        stopFlg <- true
                        realTimeEnd <- DateTime.Now
                        stopWatch.Stop()
                        printfn "rounds: %d" round
                        let realTime = realTimeEnd.Subtract(realTimeStart)
                        printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (realTime.Minutes) (realTime.Seconds) (realTime.Milliseconds)
                        let runTime = stopWatch.Elapsed
                        printfn "run time -- minutes: %d seconds: %d milliseconds: %d" (runTime.Minutes) (runTime.Seconds) (runTime.Milliseconds)
        | :? GetRumorNO as msg ->
            Actor.Context.Sender <! x.GetRumorMsgNoWithIncr()
        | :? GetState as msg ->
            Actor.Context.Sender <! new GetState(rumorMsgNO, stopFlg)
        | :? MotivateRumor as msg ->
            x.MotivateRumor()
        | _ -> printfn "unknown message"

    member x.GetRumorMsgNoWithIncr() =
        rumorMsgNO <- (rumorMsgNO + 1)
        rumorMsgNO

    member x.MotivateRumor() =
        // rumorMsgNO <- 0
        round <- (round + 1)
        mediator <! (new Send("/user/worker_" + (Random().Next(numberOfWorkers) + 1).ToString(), new Rumor(x.GetRumorMsgNoWithIncr()), true))

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
                let randomNumberOfNeighbors = 1
                for i in 1 .. randomNumberOfNeighbors do
                    let nextRumor = new Rumor(Async.RunSynchronously(mediator <? (new Send("/user/recorder", new GetRumorNO(), true)), -1))
                    mediator <! (new Send("/user/worker_" + (x.GetRandomNeighbor()).ToString(), nextRumor, true))
                    mediator <! (new Send("/user/recorder", new GetRumor(), true))
            else
                let state: GetState = Async.RunSynchronously(mediator <? (new Send("/user/recorder", new GetState(), true)), -1)
                if state.ALL_GET_RUMOR_FLG = false && state.LAST_RUMOR_MSG_NO = msg.NO then
                    mediator <! (new Send("/user/recorder", new MotivateRumor(), true))
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