namespace Actor

open System.Linq
open System.Collections.Generic

open Akka.Actor
open Akka.FSharp

open ToolsKit
open Msgs

type RandomControllerActor(numberOfClients: int) =
    inherit Actor()

    let context = Actor.Context

    let clients = new List<IActorRef>()
    let mutable clientNames: List<string> = null

    let printer = context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/printer")

    override this.PreStart() = 
        printer <! "RandomController Starts"
        let nameSet = new HashSet<string>()
        while nameSet.Count <> numberOfClients do
            nameSet.Add(Tools.getRandomString(6, 20)) |> ignore
        clientNames <- nameSet.ToList()

        for name in clientNames do
            let clientActor = context.System.ActorOf(Props(typeof<ClientActor>, [| name :> obj |]), name)
            clients.Add(clientActor) |> ignore

    override this.OnReceive message =
        match box message with
        | :? RegisterCall as msg ->
            for client in clients do
                client <! new RegisterOperation()
            printfn ""
        | :? string as msg ->
            printfn "[%s]:[%s]" (Actor.Context.Sender.Path.ToStringWithAddress()) msg
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name
