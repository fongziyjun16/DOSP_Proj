module ChordManagerActor

open System
open System.Linq
open System.Collections.Generic

open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Tools
open Msgs

type ChordManagerActor(totalResourceRequest: int) =
    inherit Actor()

    let mutable resourceRequestCounter = 0;
    let mutable stepCounter = 0

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message =
        match box message with
        | :? AddNewChordNode as msg ->
            table.Add(msg.SHA1CODE, msg.IP)
        | :? NotifyNodeContext as msg ->
            let entries = table.ToList()
            for i in 0 .. entries.Count - 1 do
                let ipName = buildNodeNameByIP(entries.[i].Value)
                if i = 0 then
                    mediator <! new Send("/user/" + ipName, new ContextInfo(entries.[entries.Count - 1].Value, entries.[i + 1].Value), true)
                else if i = entries.Count - 1 then
                    mediator <! new Send("/user/" + ipName, new ContextInfo(entries.[i - 1].Value, entries.[0].Value), true)
                else
                    mediator <! new Send("/user/" + ipName, new ContextInfo(entries.[i - 1].Value, entries.[i + 1].Value), true)
            keys <- table.Keys |> List<string>
            ips <- table.Values |> List<string>
            Actor.Context.Sender <! true
        | :? NotifyNodeRequest as msg ->
            for ip in ips do
                mediator <! new Send("/user/" + buildNodeNameByIP(ip), new IntervalRequest(), true)
        | :? NodeFoundResource as msg ->
            resourceRequestCounter <- resourceRequestCounter + 1
            stepCounter <- stepCounter + msg.STEPS
            if resourceRequestCounter = totalResourceRequest then
                let avgSteps = (double) stepCounter / (double) totalResourceRequest
                let printingInfo = "the average number of hops is " + avgSteps.ToString()
                mediator <! new Send("/user/printer", printingInfo, true)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name