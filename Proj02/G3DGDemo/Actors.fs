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

type RecorderActor(structure: Structure) =
    inherit Actor()

    let numberOfWorkers = structure.LENGTH * structure.WIDTH * structure.HEIGHT
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
            mediator <! new Send("/user/randomRouter", msg, true)
        | :? GetRumor as msg ->
            getRumorCounter <- (getRumorCounter + 1)
            x.ReportPercentage(msg.NAME)
            if getRumorCounter = numberOfWorkers then
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                x.ReportTime()
                mediator <! new Send("/user/broadcastRouter", new AllStop(), true)
        | _ -> printfn "unknown message"

    member x.ReportPercentage(name: string) =
        // mediator <! new Send("/user/printer", name + " get rumor", true)
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

type G3DWorkerActor(position: Position, structure: Structure, rumorLimit: int) =
    inherit Actor()

    let directions = [| new Position(1, 0, 0); new Position(0, 1, 0); 
                        new Position(0, 0, 1); new Position(-1, 0, 0); 
                        new Position(0, -1, 0); new Position(0, 0, -1) 
                     |]

    let mutable switch = true
    let mutable startSendOut = false

    let mutable getRumorCounter = 0;

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let sendOut(msg: Rumor) =
        let mutable counter = 0
        async {
            while switch do
                let mutable getFlg = false
                let mutable neighbor = ""
                while getFlg = false do
                    let direction = directions.[Random().Next(6)]
                    let neighborPosition = new Position(position.X + direction.X, position.Y + direction.Y, position.Z + direction.Z)
                    if neighborPosition.X >= 1 && neighborPosition.X <= structure.LENGTH &&
                       neighborPosition.Y >= 1 && neighborPosition.Y <= structure.WIDTH &&
                       neighborPosition.Z >= 1 && neighborPosition.Z <= structure.HEIGHT then
                        getFlg <- true
                        neighbor <- "worker_" + neighborPosition.X.ToString() + "_" + neighborPosition.Y.ToString() + "_" + neighborPosition.Z.ToString()

                mediator <! new Send("/user/" + neighbor, msg, true)
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
        | :? AllStop as msg ->
            switch <- false
        | _ -> printfn "unknown message"

    member x.ReportRumor() =
        let name = "worker_" + position.X.ToString() + "_" + position.Y.ToString() + "_" + position.Z.ToString()
        mediator <! new Send("/user/recorder", new GetRumor(name), true)

    member x.StartTaskWorker() =
        if startSendOut = false then
            startSendOut <- true
            sendOut(new Rumor()) |> Async.StartAsTask |> ignore
