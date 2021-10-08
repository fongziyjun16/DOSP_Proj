module GI3DGSystem

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

type StartRumor =
    struct
    end

type Rumor = 
    struct
    end

type GetRumor =
    struct
        val NAME: string
        new (name: string) ={
            NAME = name
        }
    end

type AllStop =
    struct
    end

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

type GI3DWorkerActor(position: Position, structure: Structure, rumorLimit: int) =
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
                mediator <! new Send("/user/randomRouter", msg, true)
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

type GI3DG(length: int, width: int, height: int, rumorLimit: int) =
    
    let systemName = "GI3DGSystem"

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
                                    seed-nodes = [""akka.tcp://GI3DGSystem@localhost:9090""]
                                }
                            }
                        ")
    
    let GI3DGSystem = System.create systemName (configuration)

    let structure = new Structure(length, width, height)

    let recorder = GI3DGSystem.ActorOf(Props(typeof<RecorderActor>, [| structure :> obj |]), "recorder")

    do GI3DGSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore

    let workerList = new Collections.Generic.List<string>()

    do for i in 1 .. length do
        for j in 1 .. width do
            for k in 1 .. height do
                let position = new Position(i, j, k)
                let name = "worker_" + i.ToString() + "_" + j.ToString() + "_" + k.ToString()
                GI3DGSystem.ActorOf(Props(typeof<GI3DWorkerActor>, [| position :> obj; structure :> obj; rumorLimit :> obj |]), name) |> ignore
                workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(GI3DGSystem).Mediator

    let broadcastRouter = GI3DGSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    do mediator <! new Put(broadcastRouter)

    let randomRouter = GI3DGSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    do mediator <! new Put(randomRouter)

    do recorder <! new StartRumor()

