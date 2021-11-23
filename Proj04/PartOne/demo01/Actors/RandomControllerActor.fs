namespace Actor

open System
open System.Collections.Generic

open Akka.Actor
open Akka.FSharp

open ToolsKit
open Msgs

type RandomControllerActor(numberOfClients: int) =
    inherit Actor()

    let random = new Random()
    let context = Actor.Context

    let clients = new List<IActorRef>()
    let clientLoginStatus = new Dictionary<string, bool>()

    let printer = context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/printer")

    override this.PreStart() = 
        printer <! "RandomController Starts"
        let nameSet = new HashSet<string>()
        while nameSet.Count <> numberOfClients do
            let name = Tools.getRandomString(6, 20)
            if nameSet.Add(name) then
                clientLoginStatus.Add(name, false)

        for name in clientLoginStatus.Keys do
            let clientActor = context.System.ActorOf(Props(typeof<ClientActor>, [| name :> obj |]), name)
            clients.Add(clientActor) |> ignore

    override this.OnReceive message =
        match box message with
        | :? RegisterCall as msg ->
            for client in clients do
                client <! new RegisterOperation()
            while Tools.getRegiteredClientNumber() <> clients.Count do
                ()
        | :? LoginLogoutTest as msg ->
            async {
                while true do
                    for client in clients do
                        let name = client.Path.Name
                        let sign = random.Next(1, 10)
                        if sign >= 1 && sign <= 7 then
                            clientLoginStatus.[name] <- not clientLoginStatus.[name]
                            let flg = clientLoginStatus.[name]
                            if flg then
                                client <! new LoginOperation()
                            else 
                                client <! new LogoutOperation()
                    do! Async.Sleep(5000)
            } |> Async.StartAsTask |> ignore
        | :? CLientPostTest as msg ->
            let starA = clients.[random.Next(clients.Count)]
            let starB = clients.[random.Next(clients.Count)]
            for client in clients do
                if client.Path.Name <> starA.Path.Name then
                    client <! new SubscribeOperation(starA.Path.Name)
            starA <! new PostTweetOperation(false)
            starB <! new PostTweetOperation(true)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name
