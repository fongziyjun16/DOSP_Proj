module Actors

open System
open Akka.Actor
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type PrinterActor() =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override on.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override on.OnReceive message =
        match box message with
        | :? string as msg ->
            printfn "%s" msg
        | _ -> printfn "unknown message"

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()

    let mutable getRumorCounter = 0;

    let mutable realTimeStart = DateTime.Now
    let mutable realTimeEnd = DateTime.Now
    let stopWatch = new Diagnostics.Stopwatch()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            realTimeStart <- DateTime.Now
            stopWatch.Start()
            mediator <! new Send("/user/randomRouter", new StartRumor(), true)
        | :? GetRumor as msg ->
            getRumorCounter <- (getRumorCounter + 1)
            x.ReportPercentage()
            if getRumorCounter = numberOfWorkers then
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                x.ReportTime()
                mediator <! new Send("/user/broadcastRouter", new AllStop(), true)
        | :? Motivation as msg ->
            if getRumorCounter < numberOfWorkers then
                mediator <! new Send("/user/randomRouter", new StartRumor(), true)
        | _ -> printfn "unknown message"

    member x.ReportPercentage() =
        let percentage = ((double) getRumorCounter / (double) numberOfWorkers) * 100.0
        if percentage % 20.0 = 0.0 then
            mediator <! new Send("/user/printer", percentage.ToString() + " %", true)
    
    member x.ReportTime() =
        let realTime = realTimeEnd.Subtract(realTimeStart)
        // printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (realTime.Minutes) (realTime.Seconds) (realTime.Milliseconds)
        let realTimeInfo = "real time -- minutes: " + (realTime.Minutes).ToString() + " seconds: " + (realTime.Seconds).ToString() + " milliseconds: " + (realTime.Milliseconds).ToString()
        mediator <! new Send("/user/printer", realTimeInfo, true)
        let runTime = stopWatch.Elapsed
        // printfn "run time -- minutes: %d seconds: %d milliseconds: %d" (runTime.Minutes) (runTime.Seconds) (runTime.Milliseconds)
        let runTimeInfo = "run time -- minutes: " + (runTime.Minutes).ToString() + " seconds: " + (runTime.Seconds).ToString() + " milliseconds: " + (runTime.Milliseconds).ToString()
        mediator <! new Send("/user/printer", runTimeInfo, true)

type GLWorkerActor(id: int, numberOfWorkers: int, rumorLimit: int) =
    inherit Actor()

    let mutable switch = true
    let mutable leftDone = false
    let mutable rightDone = false
    let mutable reportedGetRumor = false
    let mutable getRumorCounter = 0;

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let mutable startSendOut = false
    let sendOut(msg: Rumor) =
        let mutable counter = 0
        async {
            while switch do
                let mutable neighbor = "worker_"
                if id = 1 then neighbor <- (neighbor + "2")
                else if id = numberOfWorkers then neighbor <- (neighbor + (numberOfWorkers - 1).ToString())
                else
                    let randomDirection = Random().Next(0, 2)
                    if randomDirection = 0 then
                        neighbor <- (neighbor + (id - 1).ToString())
                    else
                        neighbor <- (neighbor + (id + 1).ToString())

                mediator <! new Send("/user/" + neighbor, new Rumor(id), true)
                counter <- (counter + 1)
                if counter = rumorLimit then
                    counter <- 0
                    do! Async.Sleep(1)
            startSendOut <- false
        }

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            x.StartTaskWorker()
            x.ReportRumor()
        | :? Rumor as msg ->
            if getRumorCounter < rumorLimit then
                getRumorCounter <- (getRumorCounter + 1)
                x.StartTaskWorker()
                if getRumorCounter = 1 then
                    x.ReportRumor()
                else if getRumorCounter = rumorLimit then
                    switch <- false
            else
                mediator <! new Send("/user/worker_" + msg.FROM.ToString(), new IAmDone(id), true)
        | :? AllStop as msg ->
            switch <- false
        | :? IAmDone as msg ->
            if (id = 1 && msg.ID = 2) || (id = numberOfWorkers && msg.ID = (numberOfWorkers - 1)) then
                mediator <! new Send("/user/recorder", new Motivation(), true)
            else
                if leftDone && rightDone then
                    mediator <! new Send("/user/recorder", new Motivation(), true)
                else
                    if msg.ID < id then leftDone <- true
                    else rightDone <- true
                    
        | _ -> printfn "worker unknown message"

    member x.ReportRumor() =
        if reportedGetRumor = false then
            reportedGetRumor <- true
            mediator <! new Send("/user/recorder", new GetRumor(), true)

    member x.StartTaskWorker() =
        if startSendOut = false then
            switch <- true
            startSendOut <- true
            sendOut(new Rumor()) |> Async.StartAsTask |> ignore

