module ChordNodeActor

open System
open System.Collections.Generic

open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Tools
open Msgs

type ChordNodeActor(ip: string, nubmerOfRequest: int) =
    inherit Actor()

    let mutable predecessor = ""
    let mutable successor = ""
    let resources = new HashSet<string>()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message =
        match box message with
        | :? ContextInfo as msg ->  // from Chord Manager
            predecessor <- msg.PREDECESSOR
            successor <- msg.SUCCESSOR
        | :? AddNewResource as msg -> // from Resource Manager
            resources.Add(msg.NAME) |> ignore
        | :? IntervalRequest as msg ->
            let mutable requestCounter = 0
            async {
                while requestCounter <> nubmerOfRequest do
                    requestCounter <- requestCounter + 1

                    // get a random resource not int this chord node
                    let mutable resource = resourceList.[random.Next(0, resources.Count)]
                    while resources.Contains(resource) do
                        resource <- resourceList.[random.Next(0, resources.Count)]

                    let checkResourceMsg = new CheckResource(resource, ip)
                    checkResourceMsg.IncrStep()

                    mediator <! new Send("/user/" + buildNodeNameByIP(successor), checkResourceMsg, true)
                    do! Async.Sleep(1000) // one request per second
            } |> Async.StartAsTask |> ignore
        | :? CheckResource as msg ->
            if resources.Contains(msg.GetResource()) then
                mediator <! new Send("/user/chordManager", new NodeFoundResource(ip, msg.GetStep()), true)
            else
                msg.IncrStep()
                mediator <! new Send("/user/" + buildNodeNameByIP(successor), msg, true)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name