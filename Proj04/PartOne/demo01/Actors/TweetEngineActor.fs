namespace Actor

open System
open System.Collections.Generic
open System.Data.SQLite

open Akka.FSharp

open ToolsKit
open Entities
open DAO
open Msgs

type TweetEngineActor() =
    inherit Actor()

    let context = Actor.Context
    let random = new Random()

    let getNewOpenDBConnection(): SQLiteConnection = 
        let newConnection = new SQLiteConnection("Data Source=./resources/tweet_sys.db")
        newConnection.Open()
        newConnection

    let accountDAO = new AccountDAO(getNewOpenDBConnection())
    let followDAO = new FollowDAO(getNewOpenDBConnection())
    let hashtagDAO = new HashtagDAO(getNewOpenDBConnection())
    let tweetDAO = new TweetDAO(getNewOpenDBConnection())
    let tweetMentionDAO = new TweetMentionDAO(getNewOpenDBConnection())
    let tweetHashtagDAO = new TweetHashtagDAO(getNewOpenDBConnection())
    
    let printer = context.System.ActorSelection(context.Self.Path.ToStringWithAddress() + "/printer")

    let getClientActor(name: string) = 
        context.System.ActorSelection(context.Self.Path.ToStringWithAddress() + name)

    let loginSet = new HashSet<string>()

    override this.OnReceive message =
        let sender = this.Sender
        match box message with
        // clients registration
        | :? RegisterInfo as msg ->
            let flg = accountDAO.insert(new Account(msg.NAME))
            if flg then 
                sender <! new RegisterSuccessInfo()
                loginSet.Add(msg.NAME) |> ignore
                Tools.addNewClient(msg.NAME)
        // clients login & logout
        | :? LoginInfo as msg ->
            loginSet.Add(msg.NAME) |> ignore
            // deliver new tweets
            let tweets = tweetDAO.getTweetsByCreators(followDAO.getFollowsByName(msg.NAME))
            sender <! new DeliverTweetsOperation(tweets)
        | :? LogoutInfo as msg ->
            loginSet.Remove(msg.NAME) |> ignore
        // clients subscribe
        | :? SubscribeInfo as msg ->
            followDAO.insert(new Follow(Tools.getRandomClient(), msg.FOLLOWER)) |> ignore
        // clients post tweet
        | :? PostTweetInfo as msg ->
            let mentions = new List<string>()
            for i in 1 .. msg.NUMBEROFMENTIONS do
                mentions.Add(Tools.getRandomClient()) |> ignore

            let hashtags = msg.HASHTAGS
            for i in 1 .. msg.NUMBEROFEXISTINGHASHTAGS do
                let existingHashtag = Tools.getRandomHashtag()
                if existingHashtag.Length > 0 then
                    hashtags.Add(existingHashtag)

            tweetDAO.insert(new Tweet(msg.NAME, msg.CONTENT)) |> ignore

            let tweetID = tweetDAO.getLastInsertRowID()

            // add mention to db
            for mention in mentions do
                tweetMentionDAO.insert(new TweetMention(tweetID, mention)) |> ignore

            // add hashtag to db
            for hashTag in hashtags do
                let queryHashtag = hashtagDAO.getHashtagByTopic(hashTag)
                if queryHashtag.TOPIC.Length = 0 then
                    // new hashtag
                    let mutable hashTagID = hashtagDAO.getLastInsertRowID()
                    tweetHashtagDAO.insert(new TweetHashtag(tweetID, hashTagID)) |> ignore
                else
                    // existing hashtag
                    tweetHashtagDAO.insert(new TweetHashtag(tweetID, queryHashtag.ID)) |> ignore

            // whether retweet
            let mutable retweetID = -1
            if msg.RETWEETFLAG then
                retweetID <- random.Next(tweetID)
                tweetDAO.updateTweetRetweetIDByTweetID(tweetID, retweetID) |> ignore
                
            // deliver to follower
            let followers = followDAO.getFollowersByName(msg.NAME)
            let deliverTweetOperation = new DeliverTweetOperation(msg.NAME, msg.CONTENT, retweetID)
            for follower in followers do
                getClientActor(follower) <! deliverTweetOperation
        // query follow tweets
        | :? QueryFollowInfo as msg ->
            // query one of follow tweets
            let follows = followDAO.getFollowsByName(msg.FOLLOWER)
            let follow = follows.[random.Next(follows.Count)]
            let tweets = tweetDAO.getTweetsByCreator(follow)
            getClientActor(msg.FOLLOWER) <! new QueryFollowResult(tweets)
        // query mention tweets
        | :? QueryMentionInfo as msg ->
            // query mention tweets
            let tweetIDs = tweetMentionDAO.getTweetIDsByName(msg.NAME)
            let tweets = tweetDAO.getTweetsByTweetIDs(tweetIDs)
            getClientActor(msg.NAME) <! new QueryMentionResult(tweets)
        // query hashtag tweets
        | :? QueryHashtagsInfo as msg ->
            // query tweets with hashtag
            let hashtag = Tools.getRandomHashtag()
            let tweetIDs = tweetHashtagDAO.getTweetIDsByHashtag(hashtag)
            let tweets = tweetDAO.getTweetsByTweetIDs(tweetIDs)
            getClientActor(msg.NAME) <! new QueryHashtagsResult(tweets)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name

