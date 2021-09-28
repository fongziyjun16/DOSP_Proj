module Actors

open System
open Akka.FSharp
open Akka.Cluster
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()
    
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
        | :? EndRumor as msg ->
            if stopTellingCounter <> numberOfWorkers then
                stopTellingCounter <- (stopTellingCounter + 1)
            else 
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                printfn "stop rumor. real time span: %A ; process run how long: %A" 
                        (realTimeEnd.Subtract(realTimeStart)) 
                        stopWatch.Elapsed.Milliseconds
        | :? RumorCounter as msg ->
            printfn "from: %s dest: %s counter: %d" msg.FROM msg.DEST msg.COUNTER
        | _ -> printfn "unknown message"


type WorkerActor(id: int, numberOfWorkers: int, rumorTimes: int) =
    inherit Actor()

    let addrs = new Collections.Generic.List<int>()
    let mutable rumorCounter = 0
    let mutable flg = false

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! (new Put(Actor.Context.Self))
        for i in 1 .. numberOfWorkers do
            if i <> id then
                addrs.Add(i)

    override this.OnReceive message =
        match box message with
        | :? Rumor as msg ->
            if rumorCounter <> rumorTimes then
                rumorCounter <- (rumorCounter + 1)
                let nextID = (addrs.[Random().Next(numberOfWorkers - 1)])
                mediator <! (new Send("/user/worker_" + nextID.ToString(), msg, true))
                mediator <! (new Send("/user/recorder", new RumorCounter("worker_" + id.ToString(), "worker_" + nextID.ToString(), rumorCounter), true))
            else
                if flg = false then
                    flg <- true
                    mediator <! (new Send("/user/recorder", new EndRumor(), true))
        | _ -> printfn "unknown message"

    override this.PostStop() =
        ()
        