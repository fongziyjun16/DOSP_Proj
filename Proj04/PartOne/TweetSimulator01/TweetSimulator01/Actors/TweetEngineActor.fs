namespace Actor

open System
open System.Linq
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
        let newConnection = new SQLiteConnection("Data Source=./resources/tweet_sys.db;Version=3;")
        newConnection.Open()
        newConnection

    let accountDAO = new AccountDAO(getNewOpenDBConnection())
    let followDAO = new FollowDAO(getNewOpenDBConnection())
    let hashtagDAO = new HashtagDAO(getNewOpenDBConnection())
    let tweetDAO = new TweetDAO(getNewOpenDBConnection())
    let tweetMentionDAO = new TweetMentionDAO(getNewOpenDBConnection())
    let tweetHashtagDAO = new TweetHashtagDAO(getNewOpenDBConnection())
    
    let printer = context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/printer")

    let getClientActor(name: string) = 
        context.System.ActorSelection(context.Parent.Path.ToStringWithAddress() + "/" + name)

    let loginSet = new HashSet<string>()

    let buildUpRawTweet(tweet: Tweet): TweetDTO = 
        let mentions = tweetMentionDAO.getMentionsByTweetID(tweet.ID)
        let hashtagIDs = tweetHashtagDAO.getHashtagIDByTweetID(tweet.ID)
        let hashtags = hashtagDAO.getTopicsByHashtagIDs(hashtagIDs)
        new TweetDTO(tweet.ID, tweet.CREATOR, tweet.CONTENT, mentions, hashtags, tweet.RETWEETID |> function id -> if id = -1 then false else true)

    let rawTweets2Tweets(rawTweets: List<Tweet>): List<TweetDTO> = 
        let tweets = new List<TweetDTO>()
        for rawTweet in rawTweets do
            tweets.Add(buildUpRawTweet(rawTweet))
        tweets

    override this.OnReceive message =
        let sender = this.Sender
        match box message with
        // clients registration
        | :? RegisterInfo as msg ->
            let flg = accountDAO.insert(new Account(msg.NAME))
            if flg then 
                sender <! new RegisterSuccessInfo()
                Tools.addNewClient(msg.NAME)
        // clients login & logout
        | :? LoginInfo as msg ->
            loginSet.Add(msg.NAME) |> ignore
            // deliver new tweets
            (*
            let follows = followDAO.getFollowsByName(msg.NAME)
            let tweets = tweetDAO.getTweetsByCreators(follows)
            sender <! new DeliverTweetsOperation(tweets)
            *)
            printer <! msg.NAME + " login"
        | :? LogoutInfo as msg ->
            loginSet.Remove(msg.NAME) |> ignore
            printer <! msg.NAME + " logout"
        // clients subscribe
        | :? SubscribeInfo as msg ->
            followDAO.insert(new Follow(msg.FOLLOW, msg.FOLLOWER)) |> ignore
            printer <! msg.FOLLOWER + " follows " + msg.FOLLOW
        // clients post tweet
        | :? PostTweetInfo as msg ->
            let mentionSet = new HashSet<string>()
            while mentionSet.Count <> msg.NUMBEROFMENTIONS do
                let mention = Tools.getRandomClient()
                if mention <> msg.NAME then
                    mentionSet.Add(mention) |> ignore
            let mentions = mentionSet.ToList()

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
                    hashtagDAO.insert(new Hashtag(hashTag, msg.NAME)) |> ignore
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
            let tweetDTO = new TweetDTO(tweetID, msg.NAME, msg.CONTENT, mentions, hashtags, msg.RETWEETFLAG)
            let deliverTweetOperation = new DeliverTweetOperation(tweetDTO)
            for follower in followers do
                getClientActor(follower) <! deliverTweetOperation
        // query follow tweets
        | :? QueryFollowInfo as msg ->
            // query one of follow tweets
            let follows = followDAO.getFollowsByName(msg.FOLLOWER)
            let follow = follows.[random.Next(follows.Count)]
            let rawTweets = tweetDAO.getTweetsByCreator(follow)
            let tweets = rawTweets2Tweets(rawTweets)
            getClientActor(msg.FOLLOWER) <! new QueryFollowResult(tweets)
        // query mention tweets
        | :? QueryMentionInfo as msg ->
            // query mention tweets
            let tweetIDs = tweetMentionDAO.getTweetIDsByName(msg.NAME)
            let rawTweets = tweetDAO.getTweetsByTweetIDs(tweetIDs)
            let tweets = rawTweets2Tweets(rawTweets)
            getClientActor(msg.NAME) <! new QueryMentionResult(tweets)
        // query hashtag tweets
        | :? QueryHashtagsInfo as msg ->
            // query tweets with hashtag
            let hashtag = Tools.getRandomHashtag()
            let tweetIDs = tweetHashtagDAO.getTweetIDsByHashtag(hashtag)
            let rawTweets = tweetDAO.getTweetsByTweetIDs(tweetIDs)
            let tweets = rawTweets2Tweets(rawTweets)
            getClientActor(msg.NAME) <! new QueryHashtagsResult(tweets)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name

