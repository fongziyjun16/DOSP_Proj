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

type RecorderActor(numberOfWorkers: int) =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override on.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override on.OnReceive message =
        match box message with
        | :? string as msg ->
            printfn "%s" msg
        | _ -> printfn "unknown message"

type PSFNWorkerActor(id: int, numberOfWorkers: int) =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override on.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override on.OnReceive message =
        match box message with
        | :? string as msg ->
            printfn "%s" msg
        | _ -> printfn "unknown message"
