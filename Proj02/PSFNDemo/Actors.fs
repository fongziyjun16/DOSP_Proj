module Actors

open System
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

    let mutable roundsCounter = 0
    let mutable singleRumorGetCounter = 0
    let mutable getRumorCounter = 0
    let mutable numberOfWorking = numberOfWorkers

    let mutable realTimeStart = DateTime.Now
    let mutable realTimeEnd = DateTime.Now
    let stopWatch = new Diagnostics.Stopwatch()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? Start as msg ->
            realTimeStart <- DateTime.Now
            stopWatch.Start()
            mediator <! new Send("/user/broadcastRouter", new SendOut(), true)
        | :? Termination as msg ->
            numberOfWorking <- (numberOfWorking - 1)
            getRumorCounter <- (getRumorCounter + 1)

            x.ReportPercentage(msg.ID)

            if getRumorCounter = numberOfWorkers then
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                x.ReportTime()
        | :? OneRoundGet as msg ->
            if singleRumorGetCounter < numberOfWorking then
                singleRumorGetCounter <- (singleRumorGetCounter + 1)
                if singleRumorGetCounter = numberOfWorking then
                    mediator <! new Send("/user/broadcastRouter", new Calculation(), true)
                    if getRumorCounter < numberOfWorkers then
                        x.CountRounds()
                        mediator <! new Send("/user/broadcastRouter", new SendOut(), true)
        | _ -> printfn "unknown message"

    member x.ReportPercentage(id) =
        let percentage = ((double) getRumorCounter / (double) numberOfWorkers) * 100.0
        if percentage % 10.0 = 0.0 then
            mediator <! new Send("/user/printer", percentage.ToString() + " %", true)
    
    member x.ReportTime() =
        mediator <! new Send("/user/printer", "rounds: " + roundsCounter.ToString(), true)
        let realTime = realTimeEnd.Subtract(realTimeStart)
        // printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (realTime.Minutes) (realTime.Seconds) (realTime.Milliseconds)
        let realTimeInfo = "real time -- minutes: " + (realTime.Minutes).ToString() + " seconds: " + (realTime.Seconds).ToString() + " milliseconds: " + (realTime.Milliseconds).ToString()
        mediator <! new Send("/user/printer", realTimeInfo, true)
        let runTime = stopWatch.Elapsed
        // printfn "run time -- minutes: %d seconds: %d milliseconds: %d" (runTime.Minutes) (runTime.Seconds) (runTime.Milliseconds)
        let runTimeInfo = "run time -- minutes: " + (runTime.Minutes).ToString() + " seconds: " + (runTime.Seconds).ToString() + " milliseconds: " + (runTime.Milliseconds).ToString()
        mediator <! new Send("/user/printer", runTimeInfo, true)

    member x.CountRounds() =
        singleRumorGetCounter <- 0
        roundsCounter <- (roundsCounter + 1)
        


type PSFNWorkerActor(id: int) =
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

                let rumor = new Rumor(orgS, orgW)
                mediator <! new Send("/user/randomRouter", rumor, true)
                mediator <! new Send("/user/worker_" + id.ToString(), rumor, true)
        | :? Rumor as msg ->
            if consecutiveTimes < 3 then
                newS <- newS + msg.S
                newW <- newW + msg.W
                mediator <! new Send("/user/recorder", new OneRoundGet(), true)
        | :? Calculation as msg ->
            if consecutiveTimes < 3 then
                let orgRatio = orgS / orgW
                let newRatio = newS / orgW
                let changes = Math.Abs(orgRatio - newRatio) / orgRatio
                if changes <= Math.Pow(10.0, -10.0) then
                    consecutiveTimes <- (consecutiveTimes + 1)
                    if consecutiveTimes = 3 then
                        mediator <! new Send("/user/recorder", new Termination(id), true)
                else
                    consecutiveTimes <- 0
        | _ -> printfn "unknown message"