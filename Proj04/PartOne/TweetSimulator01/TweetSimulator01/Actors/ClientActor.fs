namespace Actor

open System
open System.Text
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

    let printingQueryTweets(tweets: List<SimpleTweetDTO>): string =
        let printing = new StringBuilder()
        printing.Append(" [ ") |> ignore
        for i in 0 .. tweets.Count - 1 do
            let simpleTweet = tweets.[i]
            printing.Append(simpleTweet.toString()) |> ignore
            if i <> tweets.Count - 1 then
                printing.Append(", ") |> ignore
        printing.Append(" ] ") |> ignore
        printing.ToString()
    
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

            ()
        // logout
        | :? LogoutOperation as msg ->
            login <- false
            tweetEngine <! new LogoutInfo(name)
        // randomly subscribe
        | :? SubscribeOperation as msg ->
            tweetEngine <! new SubscribeInfo(msg.FOLLOW, name)
        // post tweet
        | :? PostTweetOperation as msg ->
            let mutable numberOfMentions = -1
            let nubmerOfRegistered = Tools.getRegiteredClientNumber()
            if nubmerOfRegistered >= 11 then
                numberOfMentions <- random.Next(10)
            else
                numberOfMentions <- random.Next(nubmerOfRegistered)

            let nubmerOfNewHashtags = random.Next(5)
            let numberOfExistingHashtags = random.Next(5)

            let content = Tools.getRandomString(1, 300)
            let hashtags = new List<string>()
            for i in 1 .. nubmerOfNewHashtags do
                hashtags.Add(Tools.getRandomString(1, 20))

            tweetEngine <! new PostTweetInfo(name, content, numberOfMentions, numberOfExistingHashtags, hashtags, msg.RETWEETFLAG)
            printer <! name + " posts new one"
        // get follow post a new tweet
        | :? DeliverTweetOperation as msg ->
            let oneNewTweet = msg.TWEET
            // print tweet
            printer <! name + " gets one " + oneNewTweet.toString()
        // query follow tweets
        | :? QueryFollowOperation as msg ->
            tweetEngine <! new QueryFollowInfo(name)
        | :? QueryFollowResult as msg ->
            let tweets = msg.TWEETS
            // print
            let printing = printingQueryTweets(tweets)
            printer <! "name query follow get " + printing
        // query mention tweet
        | :? QueryMentionOperation as msg ->
            tweetEngine <! new QueryMentionInfo(name)
        | :? QueryMentionResult as msg ->
            let tweets = msg.TWEETS
            // print
            let printing = printingQueryTweets(tweets)
            printer <! "name query mention get " + printing
        // query mention tweet
        | :? QueryHashtagOperation as msg ->
            // let hashtag = Tools.getRandomHashtag()
            tweetEngine <! new QueryHashtagInfo(name(*, hashtag*))
        | :? QueryHashtagResult as msg ->
            let tweets = msg.TWEETS
            // print
            let printing = printingQueryTweets(tweets)
            printer <! "name query hashtag \"" + msg.HASHTAG + "\" get " + printing
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name

