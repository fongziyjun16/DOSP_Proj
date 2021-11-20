namespace Actor

open System
open System.Collections.Generic

open Akka.FSharp

open ToolsKit
open Entities
open Msgs

type ClientActor(name: string) =
    inherit Actor()

    let context = Actor.Context
    let tweetEngine = Actor.Context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/tweetEngine")
    let printer = Actor.Context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/printer")
    let random = new Random()

    let mutable registerFlg = false
    let mutable login = false

    override this.OnReceive message =
        match box message with
        // registration work
        | :? RegisterOperation as msg ->
            tweetEngine <! new RegisterInfo(name)
        | :? RegisterSuccessInfo as msg ->
            registerFlg <- true
            login <- true
            printer <! name + " registered"
        // login
        | :? LoginOperation as msg ->
            login <- true
            tweetEngine <! new LoginInfo(name)
        // after login get new tweets
        | :? DeliverTweetsOperation as msg ->
            let tweets = msg.TWEETS
            // print tweets

            printfn "aaa"
        // logout
        | :? LogoutOperation as msg ->
            login <- false
            tweetEngine <! new LogoutInfo(name)
        // randomly subscribe
        | :? SubscribeOperation as msg ->
            tweetEngine <! new SubscribeInfo()
        // post tweet
        | :? PostTweetOperation as msg ->
            let numberOfMentions = random.Next(51)
            let nubmerOfNewHashtags = random.Next(5)
            let numberOfExistingHashtags = random.Next(5)

            let content = Tools.getRandomString(1, 300)
            let hashtags = new List<string>()
            for i in 1 .. nubmerOfNewHashtags do
                hashtags.Add(Tools.getRandomString(1, 20))

            tweetEngine <! new PostTweetInfo(name, content, numberOfMentions, numberOfExistingHashtags, hashtags, msg.RETWEETFLAG)
        // get follow post a new tweet
        | :? DeliverTweetOperation as msg ->
            let oneNewTweet = new Tweet(msg.NAME, msg.CONTENT, msg.RETWEETID)
            // print tweet

            printfn "aaa"
        // query follow tweets
        | :? QueryFollowOperation as msg ->
            tweetEngine <! new QueryFollowInfo(name)
        | :? QueryFollowResult as msg ->
            let tweets = msg.TWEETS
            // print

            printfn "aaa"
        // query mention tweet
        | :? QueryMentionOperation as msg ->
            tweetEngine <! new QueryMentionInfo(name)
        | :? QueryMentionResult as msg ->
            let tweets = msg.TWEETS
            // print

            printfn "aaaa"
        // query mention tweet
        | :? QueryHashtagsOperation as msg ->
            tweetEngine <! new QueryHashtagsInfo(name)
        | :? QueryHashtagsResult as msg ->
            let tweets = msg.TWEETS
            // print

            printfn "aaaa"
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name

