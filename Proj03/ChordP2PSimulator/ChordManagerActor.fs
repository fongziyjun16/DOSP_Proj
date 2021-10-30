module ChordManagerActor

open System
open System.Text
open System.Collections.Generic

open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open ToolsKit
open Msgs

type ChordManagerActor(numberOfNodes: int, numberOfEachNodeRequest: int) =
    inherit Actor()

    let mutable foundResourceNodeCounter = 0
    let mutable stepsCounter = 0;
    let totalRequest = numberOfNodes + numberOfEachNodeRequest

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message =
        match box message with
        | :? CheckChordStructure as msg ->
            let table = ToolsKit.getCorrectIdentifiers()
            let identifiers = table.Keys |> List<string>
            let mutable index = 0
            let mutable flg = true
            while flg && index < identifiers.Count do
                let identifier = identifiers.[index]
                let target = Actor.Context.System.ActorSelection(Actor.Context.Self.Path.Root.ToString() + "/user/" + identifier)
                let nodeContextInfo = (string) (Async.RunSynchronously(target <? new AskNodeContext(), -1)) |> fun x ->
                                                                                                                let parts = x.Split(':')
                                                                                                                parts
                let orgContextInfo = table.[identifier].ToString() |> fun x ->
                                                                        let org = (new StringBuilder(x)).ToString(1, x.Length - 2)
                                                                        let parts = org.Split(", ")
                                                                        parts
                let partOne = (nodeContextInfo.[0]).Equals(orgContextInfo.[0])
                let partTwo = (nodeContextInfo.[1]).Equals(orgContextInfo.[1])
                if partOne <> true || partTwo <> true then
                    flg <- false
                if flg then index <- index + 1
            this.Sender <! flg
        | :? FoundResource as msg ->
            foundResourceNodeCounter <- foundResourceNodeCounter + 1
            stepsCounter <- stepsCounter + msg.STEPS
            if foundResourceNodeCounter = totalRequest then
                let avg = (double) stepsCounter / (double) (totalRequest)
                mediator <! new Send("/user/printer", "the average number of hops is " + avg.ToString(), true)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name
    