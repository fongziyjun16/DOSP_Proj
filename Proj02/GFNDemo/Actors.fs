module Actors

open System
open Akka.Actor
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()

    let mutable numberOfGetRumor = 0

    let mutable realTimeStart = DateTime.Now
    let mutable realTimeEnd = DateTime.Now
    let stopWatch = new Diagnostics.Stopwatch()

    let eventManager = Actor.Context.System.EventStream

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message = 
        match box message with
        | :? StartRumor as msg ->
            stopWatch.Start()
            mediator <! new Send("/user/worker_" + (Random().Next(numberOfWorkers) + 1).ToString(), new Rumor(), true)
        | :? EndRumor as msg ->
            if numberOfGetRumor < numberOfWorkers then
                numberOfGetRumor <- (numberOfGetRumor + 1)
                let percentage = (double) numberOfGetRumor / (double) numberOfWorkers * 100.0
                if percentage / 10.0 > 1.0 then
                    printfn "%f %%" percentage
                if numberOfGetRumor = numberOfWorkers then
                    eventManager.Publish(new AllGetRumor())
                    realTimeEnd <- DateTime.Now
                    stopWatch.Stop()
                    let realTime = realTimeEnd.Subtract(realTimeStart)
                    printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (realTime.Minutes) (realTime.Seconds) (realTime.Milliseconds)
                    let runTime = stopWatch.Elapsed
                    printfn "run time -- minutes: %d seconds: %d milliseconds: %d" (runTime.Minutes) (runTime.Seconds) (runTime.Milliseconds)
        | :? string as msg ->
            printfn "%s" msg
        | _ -> printfn "unkown message"

type SwitchActor() =
    inherit Actor()

    let mutable switch = true

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message = 
        match box message with
        | :? GetSwitch as msg ->
            Actor.Context.Sender <! switch
        | :? SetSwitch as msg ->
            switch <- msg.VALUE
        | _ -> printfn "unkown message"

type TaskProcessorActor(id: int, numberOfWorkers: int) =
    inherit Actor()

    let switch = Actor.Context.ActorSelection(Actor.Context.Parent.Path.ToStringWithAddress() + "/switch")

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message = 
        match box message with
        | :? Rumor as msg ->
            let mutable switchFlg = x.GetSwitch()
            while switchFlg do
                let neighbor = x.GetRandomNeighbor().ToString()
                mediator <! new Send("/user/worker_" + neighbor, new Rumor(), true)
                switchFlg <- x.GetSwitch()
        | _ -> printfn "unkown message"
    
    member x.GetSwitch() = 
        Async.RunSynchronously(switch <? new GetSwitch(), -1)

    member x.GetRandomNeighbor() =
        let random = new Random()
        if id = 1 then 
            random.Next(2, numberOfWorkers + 1)
        else if id = numberOfWorkers then 
            random.Next(1, numberOfWorkers)
        else 
            let candidates = [| random.Next(1,id); random.Next(id + 1, numberOfWorkers) |]
            candidates.[random.Next(0, 2)]

type GFNWorkerActor(id: int, numberOfWorkers: int, rumorLimit: int) =
    inherit Actor()

    let mutable getRumorFlg = false
    let mutable numberOfGetRumor = 0;

    let mutable taskProcessorWorkingFlg = false
    let taskProcessor = Actor.Context.ActorOf(Props(typeof<TaskProcessorActor>, [| id :> obj; numberOfWorkers :> obj |]), "taskProcessor")
    let switch = Actor.Context.ActorOf(Props(typeof<SwitchActor>), "switch")

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override x.OnReceive message = 
        match box message with
        | :? Rumor as msg ->
            if numberOfGetRumor < rumorLimit then
                numberOfGetRumor <- (numberOfGetRumor + 1)
                if getRumorFlg = false then
                    getRumorFlg <- true
                    mediator <! new Send("/user/recorder", new EndRumor(), true)
                if taskProcessorWorkingFlg = false then
                    taskProcessorWorkingFlg <- true
                    taskProcessor <! msg
                if numberOfGetRumor = numberOfWorkers then
                    switch <! new SetSwitch(false)
        | :? AllGetRumor as msg ->
            switch <! new SetSwitch(false)
        | _ -> printfn "unkown message"


