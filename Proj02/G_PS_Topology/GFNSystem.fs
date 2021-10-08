module GFNSystem

open System
open Akka.Actor
open Akka.FSharp
open Akka.Routing
open Akka.Configuration
open Akka.Cluster.Tools.PublishSubscribe

type StartRumor =
    struct
    end

type Rumor = 
    struct
    end

type GetRumor =
    struct
        val ID: int
        new (id: int) ={
            ID = id
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
            mediator <! new Send("/user/randomRouter", msg, true)
        | :? GetRumor as msg ->
            getRumorCounter <- (getRumorCounter + 1)
            x.ReportPercentage()
            if getRumorCounter = numberOfWorkers then
                realTimeEnd <- DateTime.Now
                stopWatch.Stop()
                x.ReportTime()
                mediator <! new Send("/user/broadcastRouter", new AllStop(), true)
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

type GFNWorkerActor(id: int, numberOfWorkers: int, rumorLimit: int) =
    inherit Actor()

    let mutable switch = true
    let mutable startSendOut = false
    let mutable getRumorCounter = 0;

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let sendOut(msg: Rumor) =
        let mutable counter = 0
        async {
            while switch do
                mediator <! new Send("/user/randomRouter", msg, true)
                counter <- (counter + 1)
                if counter = rumorLimit then
                    counter <- 0
                    do! Async.Sleep(1)
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
                if getRumorCounter = 1 then
                    x.ReportRumor()
                    x.StartTaskWorker()
                else if getRumorCounter = rumorLimit then
                    switch <- false
        | :? AllStop as msg ->
            switch <- false
        | _ -> printfn "unknown message"

    member x.ReportRumor() =
        mediator <! new Send("/user/recorder", new GetRumor(), true)

    member x.StartTaskWorker() =
        if startSendOut = false then
            startSendOut <- true
            sendOut(new Rumor()) |> Async.StartAsTask |> ignore

type GFN(numberOfWorkers: int, rumorLimit: int) =
    
    let systemName = "GFNSystem"

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
                                    seed-nodes = [""akka.tcp://GFNSystem@localhost:9090""]
                                }
                            }
                        ")
    
    let GFNSystem = System.create systemName (configuration)
    let recorder = GFNSystem.ActorOf(Props(typeof<RecorderActor>, [| numberOfWorkers :> obj |]), "recorder")
    do GFNSystem.ActorOf(Props(typeof<PrinterActor>), "printer") |> ignore

    let workerList = new Collections.Generic.List<string>()
    do for i in 1 .. numberOfWorkers do
        let name = "worker_" + i.ToString()
        GFNSystem.ActorOf(Props(typeof<GFNWorkerActor>, [| i :> obj; numberOfWorkers :> obj; rumorLimit :> obj |]), name) |> ignore
        workerList.Add("/user/" + name)

    let mediator = DistributedPubSub.Get(GFNSystem).Mediator

    let broadcastRouter = GFNSystem.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(workerList)), "broadcastRouter")
    do mediator <! new Put(broadcastRouter)

    let randomRouter = GFNSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workerList)), "randomRouter")
    do mediator <! new Put(randomRouter)

    do recorder <! new StartRumor()
    
