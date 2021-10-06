module Actors

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

type TaskControllerActor() =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? StartTask as msg ->
            while x.isContinueTask() do
                mediator <! new Send("/user/broadCastRouter", new SendOut(), true)
                mediator <! new Send("/user/broadCastRouter", new Calculation(), true)
        | _ -> printfn "unknown message"

    member x.isContinueTask() =
        Async.RunSynchronously(mediator <? new Send("/user/recorder", new CheckIsContinue(), true), -1)

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()

    let mutable getRumorCounter = 0;

    let mutable realTimeStart = DateTime.Now
    let mutable realTimeEnd = DateTime.Now
    let stopWatch = new Diagnostics.Stopwatch()

    let taskController = Actor.Context.ActorOf(Props(typeof<TaskControllerActor>), "taskController")
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? Start as msg ->
            realTimeStart <- DateTime.Now
            stopWatch.Start()
            taskController <! new StartTask()
        | :? Termination as msg ->
            getRumorCounter <- (getRumorCounter + 1)

            x.ReportPercentage(msg.ID)

            if getRumorCounter = numberOfWorkers then
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                x.ReportTime()
        | :? CheckIsContinue as msg ->
            if getRumorCounter < numberOfWorkers then
                Actor.Context.Sender <! true
            else
                Actor.Context.Sender <! false
        | _ -> printfn "unknown message"

    member x.ReportPercentage(id) =
        let percentage = ((double) getRumorCounter / (double) numberOfWorkers) * 100.0
        if percentage % 10.0 = 0.0 then
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

type PSFNWorkerActor(id: int, numberOfWorkers: int) =
    inherit Actor()

    let mutable consecutiveTimes = 0;

    let mutable orgS = (double) id
    let mutable orgW = 1.0
    let mutable newS = 0.0
    let mutable newW = 0.0

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? SendOut as msg ->
            if consecutiveTimes < 3 then
                orgS <- (orgS / 2.0)
                orgW <- (orgW / 2.0)

                newS <- orgS
                newW <- orgW

                mediator <! new Send("/user/randomRouter", new Rumor(orgS, orgW), true)
        | :? Rumor as msg ->
            if consecutiveTimes < 3 then
                newS <- newS + msg.S
                newW <- newW + msg.W
        | :? Calculation as msg ->
            if consecutiveTimes < 3 then
                let orgRatio = orgS / orgW
                let newRatio = newS / orgW
                let changes = Math.Abs(orgRatio - newRatio) / orgRatio
                if changes <= Math.Pow(10.0, -10.0) then
                    consecutiveTimes <- (consecutiveTimes + 1)
                    if consecutiveTimes = 3 then
                        mediator <! new Send("/user/recorder", new Termination(id), true)
                        mediator <! new Send("/user/worker_" + id.ToString(), new Termination(id), true)
                else
                    consecutiveTimes <- 0
        | _ -> printfn "unknown message"