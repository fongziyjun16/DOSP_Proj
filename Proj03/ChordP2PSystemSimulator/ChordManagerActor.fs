module ChordManagerActor

open System.Linq

open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open ToolsKit
open Msgs

type ChordManagerActor(numberOfNodes: int, numberOfEachNodeRequest: int) =
    inherit Actor()

    let totalNumberOfRequests = numberOfNodes * numberOfEachNodeRequest
    let mutable numberOfResourceFound = 0
    let mutable totalNumberOfSteps = 0

    let context = Actor.Context
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let print(info: string) =
        mediator <! new Send("/user/printer", info, true)

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message =
        match box message with
        | :? CheckChordStructure as msg ->
            let nodesContextInfo = ToolsKit.getNodesContextInfo()
            let identifiers = nodesContextInfo.Keys.ToList()
            let mutable index = 0
            let mutable flg = true
            while flg && index <= identifiers.Count - 1 do
                let identifier = identifiers.[index]
                let target = context.ActorSelection(context.Self.Path.Root.ToString() + "/user/" + identifier)
                let currentContextInfo = (string) (Async.RunSynchronously(target <? new AskContextInfo(), -1))
                let correctContextInfo = nodesContextInfo.[identifier]
                index <- index + 1
                flg <- currentContextInfo.Equals(correctContextInfo)
            if flg then ToolsKit.builtStructure()
        | :? FoundResource as msg ->
            numberOfResourceFound <- numberOfResourceFound + 1
            totalNumberOfSteps <- totalNumberOfSteps + msg.STEPS
            print(msg.PUBLISHER + " found key " + msg.KEY + " in " + msg.STEPS.ToString() + " steps ")
            if numberOfResourceFound = totalNumberOfRequests then
                let avgHops = (double) totalNumberOfSteps / (double) totalNumberOfRequests
                mediator <! new Send("/user/nodesRouter", new StopFixFingerTable(), true)
                print("the average number of hops is " + avgHops.ToString())
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name
