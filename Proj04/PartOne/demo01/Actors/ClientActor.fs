namespace Actor

open Akka.FSharp

open ToolsKit
open Entities
open Msgs

type ClientActor() =
    inherit Actor()

    let mutable name = Tools.getRandomString(10, 20)
    let tweetEngine = Actor.Context.System.ActorSelection("akka://TweetSimulator@localhost:10012/user/tweetEngine")
    let printer = Actor.Context.System.ActorSelection("akka://TweetSimulator@localhost:10012/user/printer")

    let mutable registerFlg = false
    let mutable login = false

    override this.OnReceive message =
        match box message with
        // registration work
        | :? RegisterOperationInfo as msg ->
            tweetEngine <! new RegisterInfo(name)
        | :? RegisterSuccessInfo as msg ->
            registerFlg <- true
            login <- true
        | :? RegisterFailureInfo as msg ->
            name <- Tools.getRandomString(10, 20)
            this.Self <! new RegisterOperationInfo()
        // login & logout
        | :? LoginOperation as msg ->
            login <- true
            tweetEngine <! new LoginInfo(name)
            // receive tweets from follow

        | :? LogoutOperation as msg ->
            login <- false
            tweetEngine <! new LogoutInfo(name)
        // randomly subscribe
        | :? SubscribeOperation as msg ->
            tweetEngine <! new SubscribeInfo(Tools.getRandomClient(), name)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name

