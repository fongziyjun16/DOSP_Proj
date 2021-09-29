module Actors

open System
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()

    let mutable realTimeStart = new DateTime()
    let mutable realTimeEnd = new DateTime()
    let mutable stopWatch = new Diagnostics.Stopwatch()
    let mutable endRumorCounter = 0

    let mutable stopSend = false

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            realTimeStart <- DateTime.Now
            stopWatch.Start()
            mediator <! (new Send("/user/worker_" + (x.GetRandomStartWorker()).ToString(), new Rumor(-1.0, -1.0), true))
        | :? EndRumor as msg ->
            while stopSend = false do
                endRumorCounter <- (endRumorCounter + 1)

                mediator <! (new Send("/user/worker_" + (x.GetRandomStartWorker()).ToString(), new Rumor(-1.0, -1.0), true))

                if endRumorCounter = numberOfWorkers then
                    stopSend <- true
                    realTimeEnd <- DateTime.Now
                    stopWatch.Stop()
                    let realTime = realTimeEnd.Subtract(realTimeStart)
                    printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (realTime.Minutes) (realTime.Seconds) (realTime.Milliseconds)
                    let runTime = stopWatch.Elapsed
                    printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (runTime.Minutes) (runTime.Seconds) (runTime.Milliseconds)
        | _ -> printfn "unknown message"

    member x.GetRandomStartWorker() =
        Random().Next(numberOfWorkers) + 1

type PushSumFullNetworkWorkerActor(id: int, numberOfWorkers: int) =
    inherit Actor()

    let mutable s = double(id)
    let mutable w = 1.0

    let mutable receiveCounter = 0;
    let mutable roundCounter = 0;
    let mutable stopSend = false

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))

    override x.OnReceive message =
        match box message with
        | :? Rumor as rumor ->
            if stopSend = false then
                if rumor.S = -1.0 && rumor.W = -1.0 then // first send
                    s <- s / 2.0
                    w <- w / 2.0
                    mediator <! (new Send("/user/worker_" + (x.GetRandomNeighbor()).ToString(), new Rumor(s, w), true))
                else 
                    receiveCounter <- (receiveCounter + 1)
                    let orgRatio = s / w
                    s <- (s + rumor.S)
                    w <- (w + rumor.W)
                    let newRatio = s / w
                    let changeRatio = (abs (newRatio - orgRatio)) / orgRatio
                    if changeRatio < Math.Pow(10.0, -10.0) then
                        roundCounter <- (roundCounter + 1)
                        if roundCounter = 3 then 
                            stopSend <- true
                            mediator <! (new Send("/user/recorder", new EndRumor("worker_" + id.ToString()), true))
                    else
                        roundCounter <- 0
                    s <- s / 2.0
                    w <- w / 2.0
                    if stopSend = false then 
                        mediator <! (new Send("/user/worker_" + (x.GetRandomNeighbor()).ToString(), new Rumor(s, w), true))
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

