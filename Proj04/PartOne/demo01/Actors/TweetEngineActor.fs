namespace Actor

open System.Collections.Generic
open System.Data.SQLite

open Akka.FSharp

open ToolsKit
open Entities
open DAO
open Msgs

type TweetEngineActor() =
    inherit Actor()

    let accountDAO = new AccountDAO(new SQLiteConnection("Data Source=./resources/tweet_sys.db"))
    let followDAO = new FollowDAO(new SQLiteConnection("Data Source=./resources/tweet_sys.db"))
    let hashtagDAO = new HashtagDAO(new SQLiteConnection("Data Source=./resources/tweet_sys.db"))
    let tweetDAO = new TweetDAO(new SQLiteConnection("Data Source=./resources/tweet_sys.db"))
    let tweetMentionDAO = new TweetMentionDAO(new SQLiteConnection("Data Source=./resources/tweet_sys.db"))
    let tweetHashtagDAO = new TweetHashtagDAO(new SQLiteConnection("Data Source=./resources/tweet_sys.db"))
    
    let printer = Actor.Context.System.ActorSelection("akka://TweetSimulator@localhost:10012/user/printer")

    let loginSet = new HashSet<string>()

    override this.OnReceive message =
        let sender = this.Sender
        match box message with
        // client registration
        | :? RegisterInfo as msg ->
            let flg = accountDAO.insert(new Account(msg.NAME))
            if flg then 
                sender <! new RegisterSuccessInfo()
                loginSet.Add(msg.NAME) |> ignore
            else 
                sender <! new RegisterFailureInfo()
        // client login & logout
        | :? LoginInfo as msg ->
            loginSet.Add(msg.NAME) |> ignore
            // deliver new tweets

        | :? LogoutInfo as msg ->
            loginSet.Remove(msg.NAME) |> ignore
        // client subscribe
        | :? SubscribeInfo as msg ->
            followDAO.insert(new Follow(msg.NAME, msg.FOLLOWER)) |> ignore
        
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name