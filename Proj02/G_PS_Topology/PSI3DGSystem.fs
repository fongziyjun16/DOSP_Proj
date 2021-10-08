module PSI3DGSystem

open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

type Structure =
    struct
        val LENGTH: int
        val WIDTH: int
        val HEIGHT: int
        new (length: int, width: int, height: int) = {
            LENGTH = length;
            WIDTH = width;
            HEIGHT = height
        }
    end

type Position = 
    struct
        val X: int
        val Y: int
        val Z: int
        new (x: int, y: int, z: int) = {
            X = x;
            Y = y;
            Z = z
        }
    end

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
        val NAME: string
        new (name: string) ={
            NAME = name
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

type RecorderActor(structure: Structure) =
    inherit Actor()

    let numberOfWorkers = structure.LENGTH * structure.WIDTH * structure.HEIGHT

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

            x.ReportPercentage(msg.NAME)

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

    member x.ReportPercentage(name) =
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

type PSI3DGWorkerActor(position: Position, structure: Structure, no: int) =
    inherit Actor()

    let name = "worker_" + position.X.ToString() + "_" + position.Y.ToString() + "_" + position.Z.ToString()

    let directions = [| new Position(1, 0, 0); new Position(0, 1, 0); 
                        new Position(0, 0, 1); new Position(-1, 0, 0); 
                        new Position(0, -1, 0); new Position(0, 0, -1) 
                     |]

    let mutable consecutiveTimes = 0;

    let mutable orgS = (double) no
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
                mediator <! new Send("/user/randomRouter", rumor, true)
                mediator <! new Send("/user/" + name, rumor, true)
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
                        mediator <! new Send("/user/recorder", new Termination(name), true)
                else
                    consecutiveTimes <- 0
        | _ -> printfn "unknown message"

    member x.GetRandomNeighbor() = 
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
        neighbor

type PSI3DG(length: int, width: int, height: int) =
    let systemName = "PSI3DGSystem"
    
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
                                    seed-nodes = [""akka.tcp://PSI3DGSystem@localhost:9090""]
                                }
                            }
                        ")

    let PSI3DGSystem = System.create systemName (configuration)

    let structure = new Structure(length, width, height)

    let recorder = PSI3DGSystem.ActorOf(Props(typeof<RecorderActor>, [| structure :> obj |]), "recorder")
    
    do PSI3DGSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore
    
    let workerList = new Collections.Generic.List<string>()

    let mutable no = 1

    do for i in 1 .. length do
        for j in 1 .. width do
            for k in 1 .. height do
                let position = new Position(i, j, k)
                let name = "worker_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString()
                PSI3DGSystem.ActorOf(Props(typeof<PSI3DGWorkerActor>, [| position :> obj; structure :> obj; no :> obj |]), name) |> ignore
                no <- (no + 1)
                workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(PSI3DGSystem).Mediator

    let broadcastRouter = PSI3DGSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    do mediator <! new Put(broadcastRouter)

    let randomRouter = PSI3DGSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    do mediator <! new Put(randomRouter)

    do recorder <! new Start()





