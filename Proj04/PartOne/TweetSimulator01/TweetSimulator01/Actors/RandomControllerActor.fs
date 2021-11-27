namespace Actor

open System
open System.IO
open System.Text
open System.Linq
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
    let tweetEngine = Actor.Context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/tweetEngine")

    let printer = context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/printer")

    let getRandomListExcept(number: int, index: int): List<int> =
        let randomSet = new HashSet<int>()
        let total = clients.Count
        while randomSet.Count <> number do
            let randomNumber = random.Next(total) + 1
            if randomNumber <> index then
                randomSet.Add(randomNumber) |> ignore
        randomSet.ToList()
    
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
        | :? RegisterTest as msg ->
            for client in clients do
                client <! new RegisterOperation()
                client <! new LoginOperation()
            while Tools.getRegisteredClientNumber() <> clients.Count do
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
        | :? QueryTest as msg ->
            let starA = clients.[random.Next(clients.Count)]
            let starB = clients.[random.Next(clients.Count)]
            for client in clients do
                if client.Path.Name <> starA.Path.Name then
                    client <! new SubscribeOperation(starA.Path.Name)
                    client <! new SubscribeOperation(starB.Path.Name)
            for i in 1 .. 3 do
                starA <! new PostTweetOperation(false)
                starB <! new PostTweetOperation(false)
            for client in clients do
                client <! new QueryFollowOperation()
                // client <! new QueryMentionOperation()
                // client <! new QueryHashtagOperation()
        | :? StartSimulationWithZipf as msg ->
                // client registration & login
                for client in clients do
                    client <! new RegisterOperation()
                    client <! new LoginOperation()
                // setting subscription by Zipf
                clients.[0] <! new SetIndex(0)
                for i in 1 .. clients.Count - 1 do
                    clients.[i] <! new SubscribeOperation(clients.[0].Path.Name)
                for i in 1 .. clients.Count - 1 do
                    let randomList = getRandomListExcept(clients.Count / (i + 1) |> int, i + 1)
                    clients.[i] <! new SetIndex(i)
                    for number in randomList do
                        clients.[number - 1] <! new SubscribeOperation(clients.[i].Path.Name)
                // each client action simulation
                for client in clients do
                    client <! new SimulationOperation()
                async { // clients run specific seconds
                    do! Async.Sleep(500)
                } |> Async.RunSynchronously |> ignore
                for client in clients do // stop all clients
                    client <! new StopSimulationOperation()
                // wait for all clients to stop
                while Tools.getStopSimulationNumber() <> clients.Count do
                    ()
                printer <! "all clients stop"
                tweetEngine <! new StatisticsStatus()
        | :? StatisticsStatusResult as msg ->
            let clientsStatus = msg.CLIENTS_STATUS
            let statisticsFile = @"./output/statistics.txt"
            use stream = File.Create(statisticsFile)
            for clientStatus in clientsStatus do
                let line = "[ " + clientStatus.toString() + "] \n"
                let bytes = Encoding.UTF8.GetBytes(line)
                stream.Write(bytes, 0, bytes.Length)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name
