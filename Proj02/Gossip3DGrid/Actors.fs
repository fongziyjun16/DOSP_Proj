module Actors

open System
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Msgs

type RecorderActor(gridStructure: GridStructure) =
    inherit Actor()

    let mutable getRumorCounter = 0
    let mutable allGetRumorFlg = false

    let mutable realTimeStart = new DateTime()
    let mutable realTimeEnd = new DateTime()
    let mutable stopWatch = new Diagnostics.Stopwatch()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))

    override x.OnReceive message =
        match box message with
        | :? StartRumor as msg ->
            realTimeStart <- DateTime.Now
            stopWatch.Start()
            mediator <! (new Send("/user/" + x.GetRandomWorker(), new Rumor(), true))
        | :? EndRumor as msg ->
            if allGetRumorFlg = false then
                if getRumorCounter < (gridStructure.LENGTH * gridStructure.WIDTH * gridStructure.HEIGHT) then
                    getRumorCounter <- (getRumorCounter + 1)
                else
                    allGetRumorFlg <- true
                    realTimeEnd <- DateTime.Now
                    stopWatch.Stop()
                    let realTime = realTimeEnd.Subtract(realTimeStart)
                    printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (realTime.Minutes) (realTime.Seconds) (realTime.Milliseconds)
                    let runTime = stopWatch.Elapsed
                    printfn "real time -- minutes: %d seconds: %d milliseconds: %d" (runTime.Minutes) (runTime.Seconds) (runTime.Milliseconds)

        | _ -> printfn "unknown message"

    member x.GetRandomWorker() =
        let random = new Random()
        "worker_" + random.Next(gridStructure.LENGTH).ToString() + "_" +
                    random.Next(gridStructure.WIDTH).ToString() + "_" +
                    random.Next(gridStructure.HEIGHT).ToString()

type Gossip3DGridWorkerActor(position: WorkerPosition, gridStructure: GridStructure, getRumorLimit: int) =
    inherit Actor()

    let move = [| new WorkerPosition(1, 0, 0); new WorkerPosition(-1, 0, 0); // forward backward
                  new WorkerPosition(0, 1, 0); new WorkerPosition(0, -1, 0); // right left
                  new WorkerPosition(0, 0, 1); new WorkerPosition(0, 0, -1); // up down
               |]

    let neighbors = Collections.Generic.List<WorkerPosition>()
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let mutable getRumorCounter = 0;
    let mutable getRumorFlg = false

    override x.PreStart() =
        mediator <! (new Put(Actor.Context.Self))
        for i in 0 .. 5 do
            let newPosition = new WorkerPosition(
                                    position.X + move.[i].X, 
                                    position.Y + move.[i].Y, 
                                    position.Z + move.[i].Z)

            if newPosition.X >= 0 && newPosition.X < gridStructure.LENGTH && 
               newPosition.Y >= 0 && newPosition.Y < gridStructure.WIDTH &&
               newPosition.Z >= 0 && newPosition.Z < gridStructure.HEIGHT then
                neighbors.Add(newPosition)

    override x.OnReceive message =
        match box message with
        | :? Rumor as msg ->
            if getRumorCounter < getRumorLimit then
                getRumorCounter <- (getRumorCounter + 1)
                if getRumorFlg = false then
                    getRumorFlg <- true
                    mediator <! (new Send("/user/recorder", new EndRumor(), true))
                mediator <! (new Send("/user/" + x.GetRandomNeighbor(), msg, true))
        | _ -> printfn "unknown message"

    member x.GetRandomNeighbor() =
        let numbersOfNeighbor = neighbors.Count
        let neighbor = neighbors.[Random().Next(numbersOfNeighbor)]
        "worker_" + neighbor.X.ToString() + "_" + neighbor.Y.ToString() + "_" + neighbor.Z.ToString()
