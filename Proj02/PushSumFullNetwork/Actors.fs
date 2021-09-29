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

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            realTimeStart <- DateTime.Now
            stopWatch.Start()
            let startWorkerID = Random().Next(numberOfWorkers) + 1
            mediator <! (new Send("/user/worker_" + startWorkerID.ToString(), new Rumor(-1.0, -1.0), true))
        | :? EndRumor as msg ->
            endRumorCounter <- (endRumorCounter + 1)
            printfn "%s end" msg.FROM
            if endRumorCounter = numberOfWorkers then
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                printfn "stop rumor. real time span: %A ; process run how long: %A" 
                        (realTimeEnd.Subtract(realTimeStart).Milliseconds) 
                        stopWatch.Elapsed.Milliseconds
        | _ -> printfn "unknown message"

type PushSumFullNetworkWorkerActor(id: int, numberOfWorkers: int) =
    inherit Actor()

    let mutable s = double(id)
    let mutable w = 1.0

    let addrs = new Collections.Generic.List<int>()
    let mutable receiveCounter = 0;
    let mutable roundCounter = 0;
    let mutable stopSend = false

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))
        for i in 1 .. numberOfWorkers do
            if i <> id then
                addrs.Add(i)

    override x.OnReceive message =
        match box message with
        | :? Rumor as rumor ->
            if rumor.S = -1.0 && rumor.W = -1.0 then // first send
                s <- s / 2.0
                w <- w / 2.0
                let nextWorkerID = Random().Next(numberOfWorkers - 1)
                mediator <! (new Send("/user/worker_" + (addrs.[nextWorkerID]).ToString(), new Rumor(s, w), true))
            else if stopSend = false then
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
                    let nextWorkerID = Random().Next(numberOfWorkers - 1)
                    mediator <! (new Send("/user/worker_" + (addrs.[nextWorkerID]).ToString(), new Rumor(s, w), true))
        | _ -> printfn "unknown message"