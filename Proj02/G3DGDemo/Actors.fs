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
        if percentage % 5.0 = 0.0 then
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

    override on.OnReceive message =
        match box message with
        | :? GetSwitch as msg ->
            Actor.Context.Sender <! switch
        | :? SetSwitch as msg ->
            switch <- msg.SWITCH
        | _ -> printfn "unknown message"

type TaskWorkerActor(position: Position, structure: Structure) =
    inherit Actor()

    let directions = [| new Position(1, 0, 0); new Position(0, 1, 0); 
                        new Position(0, 0, 1); new Position(-1, 0, 0); 
                        new Position(0, -1, 0); new Position(0, 0, -1) 
                     |]

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override on.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? Rumor as msg ->
            while x.GetSwitch() do
                let neighbor = x.GetRandomNeighbor()
                mediator <! new Send("/user/" + neighbor, new Rumor(), true)
        | _ -> printfn "unknown message"

    member x.GetSwitch() = 
        let switchWorker = Actor.Context.ActorSelection(Actor.Context.Parent.Path.ToStringWithAddress() + "/switchWorker")
        Async.RunSynchronously(switchWorker <? new GetSwitch(), -1)
    
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

type G3DWorkerActor(position: Position, structure: Structure, rumorLimit: int) =
    inherit Actor()

    let mutable taskWorkerStart = false
    let mutable getRumorCounter = 0;

    let taskWorker = Actor.Context.ActorOf(Props(typeof<TaskWorkerActor>, [| position :> obj; structure :> obj |]), "taskWorker")
    let switchWorker = Actor.Context.ActorOf(Props(typeof<SwitchWorker>), "switchWorker")
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            taskWorker <! new Rumor()
            x.ReportRumor()
        | :? Rumor as msg ->
            if getRumorCounter < rumorLimit then
                getRumorCounter <- (getRumorCounter + 1)
                x.StartTaskWorker()
                if getRumorCounter = 1 then
                    x.ReportRumor()
                else if getRumorCounter = rumorLimit then
                    switchWorker <! new SetSwitch(false)
        | :? AllStop as msg ->
            switchWorker <! new SetSwitch(false)
        | _ -> printfn "unknown message"

    member x.ReportRumor() =
        let name = "worker_" + position.X.ToString() + "_" + position.Y.ToString() + "_" + position.Z.ToString()
        mediator <! new Send("/user/recorder", new GetRumor(name), true)

    member x.StartTaskWorker() =
        if taskWorkerStart = false then
            taskWorkerStart <- true
            taskWorker <! new Rumor()