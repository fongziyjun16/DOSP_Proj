﻿module Actors

open System
open Akka.Actor
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type PrinterActor() =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
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
            let startID = Random().Next(1, numberOfWorkers + 1)
            mediator <! new Send("/user/worker_" + startID.ToString(), msg, true)
        | :? GetRumor as msg ->
            getRumorCounter <- (getRumorCounter + 1)

            x.ReportPercentage()

            if getRumorCounter = numberOfWorkers then
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                x.ReportTime()
                x.AllStop()
        | _ -> printfn "unknown message"

    member x.AllStop() =
        let stopMsg = new AllStop()
        for i in 1 .. numberOfWorkers do
            mediator <! new Send("/user/worker_" + i.ToString(), stopMsg, true)

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

type SwitchWorker() =
    inherit Actor()

    let mutable switch = true

    override x.OnReceive message =
        match box message with
        | :? GetSwitch as msg ->
            Actor.Context.Sender <! switch
        | :? SetSwitch as msg ->
            switch <- msg.SWITCH
        | _ -> printfn "unknown message"

type TaskWorkerActor(id: int, numberOfWorkers: int) =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? Rumor as msg ->
            while x.GetSwitch() do
                let neighbor = x.GetRandomNeighbor()
                mediator <! new Send("/user/worker_" + neighbor.ToString(), msg, true) |> ignore
        | _ -> printfn "unknown message"

    member x.GetRandomNeighbor() =
        let mutable flg = false
        let mutable randomNumber = 0
        while flg = false do
            randomNumber <- Random().Next(1, numberOfWorkers + 1)
            if randomNumber <> id then
                flg <- true
        randomNumber
    
    member x.GetSwitch() = 
        let switchWorker = Actor.Context.ActorSelection(Actor.Context.Parent.Path.ToStringWithAddress() + "/switchWorker")
        Async.RunSynchronously(switchWorker <? new GetSwitch(), -1)

type PSFNWorkerActor(id: int, numberOfWorkers: int) =
    inherit Actor()

    let mutable consecutiveTimes = 0;

    let mutable s = (double) id
    let mutable w = 1.0

    let taskWorker = Actor.Context.ActorOf(Props(typeof<TaskWorkerActor>, [| id :> obj; numberOfWorkers :> obj |]), "taskWorker")
    let switchWorker = Actor.Context.ActorOf(Props(typeof<SwitchWorker>), "switchWorker")
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            taskWorker <! new Rumor(s, w)
        | :? Rumor as msg ->
            switchWorker <! new SetSwitch(false)
            
            let orgRatio = s / w
            s <- (s + msg.S)
            w <- (w + msg.W)
            let newRatio = s / w
            if consecutiveTimes = 0 then
                consecutiveTimes <- (consecutiveTimes + 1)
            else 
                if Math.Abs(orgRatio - newRatio) / orgRatio < Math.Pow(10.0, -10.0) then
                    consecutiveTimes <- (consecutiveTimes + 1)
                    if consecutiveTimes = 3 then
                        mediator <! new Send("/user/recorder", new GetRumor(), true) 
                else
                    consecutiveTimes <- 0

            if consecutiveTimes <> 3 then
                s <- s / 2.0
                w <- w / 2.0
                taskWorker <! new Rumor(s, w)
                
        | :? AllStop as msg ->
            switchWorker <! new SetSwitch(false)
        | _ -> printfn "unknown message"

    member x.ReportRumor() =
        mediator <! new Send("/user/recorder", new GetRumor(), true)