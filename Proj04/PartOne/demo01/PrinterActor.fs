module PrinterActor

open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

type PrinterActor() =
    inherit Actor()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message =
        match box message with
        | :? string as msg ->
            printfn "[%s]:[%s]" (Actor.Context.Sender.Path.ToStringWithAddress()) msg
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name