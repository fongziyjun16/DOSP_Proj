module ResourcesManagerActor

open System
open System.Text
open System.Linq
open System.Collections.Generic

open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open Tools
open Msgs

type ResourcesManagerActor(numberOfResources: int) =
    inherit Actor()

    let random = new Random()
    let letters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"

    let resourceSet = new HashSet<string>()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

        while resourceSet.Count <> numberOfResources do
            let randomLength = random.Next(10, 41)
            let randomSB = new StringBuilder()
            for i in 1 .. randomLength do
                randomSB.Append(letters.[random.Next(0, letters.Length)]) |> ignore
            resourceSet.Add(randomSB.ToString()) |> ignore
        resourceList <- resourceSet.ToList()

    override this.OnReceive message = 
        match box message with
        | :? AssignAllResources as msg ->
            for resource in resourceList do
                let successor = findSuccessor(getSHA1(resource))
                let ip = table.[successor]
                mediator <! new Send("/user/" + buildNodeNameByIP(ip), new AddNewResource(resource), true)
            Actor.Context.Sender <! true
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name