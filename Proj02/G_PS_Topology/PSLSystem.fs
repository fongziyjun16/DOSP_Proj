module PSLSystem

open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

type Start =
    struct
    end

type StartTask =
    struct
    end

type SendOut =
    struct
    end

type CheckIsContinue =
    struct
    end

type Rumor =
    struct
        val S: double
        val W: double
        new (s: double, w: double) = {
            S = s;
            W = w
        }
    end

type Calculation =
    struct
    end

type OneRoundGet =
    struct
    end

type Termination =
    struct
        val ID: int
        new (id: int) ={
            ID = id
        }
    end

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
        
type PSLWorkerActor(id: int, numberOfWorkers: int) =
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
                mediator <! new Send("/user/" + x.GetRandomNeighbor(), rumor, true)
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

    member x.GetRandomNeighbor() =
        let mutable neighbor = "worker_"
        if id = 1 then neighbor <- (neighbor + "2")
        else if id = numberOfWorkers then neighbor <- (neighbor + (numberOfWorkers - 1).ToString())
        else
            let randomDirection = Random().Next(0, 2)
            if randomDirection = 0 then
                neighbor <- (neighbor + (id - 1).ToString())
            else
                neighbor <- (neighbor + (id + 1).ToString())
        neighbor

type PSL(numberOfWorkers: int) =
    
    let systemName = "PSLSystem"

    let configuration = ConfigurationFactory.ParseString(@"
                            akka {
                                actor.provider = cluster
                                remote {
                                    dot-netty.tcp {
                                        port = 9090
                                        hostname = localhost
                                    }
                                }
                                cluster {
                                    seed-nodes = [""akka.tcp://PSLSystem@localhost:9090""]
                                }
                            }
                        ")

    let PSLSystem = System.create systemName (configuration)

    let recorder = PSLSystem.ActorOf(Props(typeof<RecorderActor>, [| numberOfWorkers :> obj |]), "recorder")
    
    do PSLSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore
    
    let workerList = new Collections.Generic.List<string>()

    do for i in 1 .. numberOfWorkers do
        let name = "worker_" + i.ToString()
        PSLSystem.ActorOf(Props(typeof<PSLWorkerActor>, [| i :> obj; numberOfWorkers :> obj |]), name) |> ignore
        workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(PSLSystem).Mediator

    let broadcastRouter = PSLSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    do mediator <! new Put(broadcastRouter)

    let randomRouter = PSLSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    do mediator <! new Put(randomRouter)

    do recorder <! new Start()






