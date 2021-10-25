module PrinterActor

open System

open Akka.FSharp

open Msgs

type PrinterActor() =
    inherit Actor()

    override this.OnReceive message =
        match box message with
        | :? PrintAvgHops as msg ->
            printfn "the average number of hops is %f" msg.AVGHOPS
        | _ -> printfn "%s get unknown message" Actor.Context.Self.Path.Name