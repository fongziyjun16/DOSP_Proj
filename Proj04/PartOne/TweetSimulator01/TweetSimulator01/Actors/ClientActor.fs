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
    
    let mutable index = 0

    let mutable registerFlg = false
    let mutable login = false

    let mutable simulationWork = true
    
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
            // login <- true
            printer <! name + " registered"
        // login
        | :? LoginOperation as msg ->
            login <- true
            tweetEngine <! new LoginInfo(name)
        // after login get new tweets
        | :? DeliverTweetsOperation as msg ->
            if login then
                let tweets = msg.TWEETS
                // print tweets

                ()
        // logout
        | :? LogoutOperation as msg ->
            login <- false
            tweetEngine <! new LogoutInfo(name)
        // randomly subscribe
        | :? SubscribeOperation as msg ->
            if login then
                tweetEngine <! new SubscribeInfo(msg.FOLLOW, name)
        | :? SetIndex as msg ->
            index <- msg.INDEX
        // post tweet
        | :? PostTweetOperation as msg ->
            if login then
                let mutable numberOfMentions = -1
                let numberOfRegistered = Tools.getRegisteredClientNumber()
                if numberOfRegistered >= 11 then
                    numberOfMentions <- random.Next(10)
                else
                    numberOfMentions <- random.Next(numberOfRegistered)

                let numberOfNewHashtags = random.Next(5)
                let numberOfExistingHashtags = random.Next(5)

                let content = Tools.getRandomString(1, 300)
                let hashtags = new List<string>()
                for i in 1 .. numberOfNewHashtags do
                    hashtags.Add(Tools.getRandomString(1, 20))

                tweetEngine <! new PostTweetInfo(name, content, numberOfMentions, numberOfExistingHashtags, hashtags, msg.RETWEETFLAG)
                // printer <! name + " posts new one"
        // get follow post a new tweet
        | :? DeliverTweetOperation as msg ->
            if login then
                let oneNewTweet = msg.TWEET
                // print tweet
                printer <! name + " gets one " + oneNewTweet.toSimpleString()
        // query follow tweets
        | :? QueryFollowOperation as msg ->
            if login then
                tweetEngine <! new QueryFollowInfo(name)
        | :? QueryFollowResult as msg ->
            let tweets = msg.TWEETS
            // print
            let printing = printingQueryTweets(tweets)
            printer <! "name query follow get " + printing
        // query mention tweet
        | :? QueryMentionOperation as msg ->
            if login then
                tweetEngine <! new QueryMentionInfo(name)
        | :? QueryMentionResult as msg ->
            let tweets = msg.TWEETS
            // print
            let printing = printingQueryTweets(tweets)
            printer <! "name query mention get " + printing
        // query mention tweet
        | :? QueryHashtagOperation as msg ->
            if login then
            // let hashtag = Tools.getRandomHashtag()
                tweetEngine <! new QueryHashtagInfo(name(*, hashtag*))
        | :? QueryHashtagResult as msg ->
            let tweets = msg.TWEETS
            // print
            let printing = printingQueryTweets(tweets)
            printer <! "name query hashtag \"" + msg.HASHTAG + "\" get " + printing
        | :? SimulationOperation as msg ->
            printer <! name + " starts simulation work"
            async {
                while simulationWork do
                    if login then
                        let postSign = random.Next(index)
                        if postSign = 0 then
                            let retweetSign = random.Next(2) |> fun sign -> sign = 0 
                            context.Self <! new PostTweetOperation(retweetSign)
                            
                        let randomOperation = random.Next(7)
                        // logout 0
                        // query follow 1 2 
                        // query mention 3 4 
                        // query hashtag 5 6
                        if randomOperation = 0 then
                            context.Self <! new LogoutOperation()
                        else if randomOperation = 1 || randomOperation = 2 then
                            context.Self <! new QueryFollowOperation()
                        else if randomOperation = 3 || randomOperation = 4 then
                            context.Self <! new QueryMentionOperation()
                        else if randomOperation = 5 || randomOperation = 6 then
                            context.Self <! new QueryHashtagOperation()
                    else
                        context.Self <! new LoginOperation()
                    do! Async.Sleep(1)
            } |> Async.StartAsTask |> ignore
        | :? StopSimulationOperation as msg ->
            simulationWork <- false
            printer <! name + "stop simualtion"
            // Tools.addStopSimulation()
            tweetEngine <! new StopSimulationInfo()
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name

