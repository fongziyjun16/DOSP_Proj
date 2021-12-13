namespace SimpleTwitter

open System
open System.Text
open System.Collections.Generic
open WebSharper
open WebSharper.AspNetCore.WebSocket.Server

module RealTimeServer =
    
    let random = new Random()
    
    let usernameToken = new Dictionary<string, string>()
    let tokenClient = new Dictionary<string, WebSocketClient<S2CMessage, C2SMessage>>()
    
    let addNewUsernameToken(username: string, token: string) =
        if usernameToken.ContainsKey(username) = false then
            usernameToken.Add(username, token)
        else if usernameToken.[username].Equals(token) = false then
            usernameToken.Remove(username) |> ignore
            usernameToken.Add(username, token)
    
    let checkLogin(username: string, token: string): bool =
        if usernameToken.ContainsKey(username) &&
           usernameToken.[username] = token then
            true
        else
            false
    
    let buildTweetsString(tweets: List<Tweet>): string =
        let result = new StringBuilder()
        for tweet in tweets do
            let qryTweet: TweetInfo = {
                creator = tweet.CREATOR
                content = tweet.CONTENT
                retweetID = tweet.RETWEETID
            }
            result.Append(Json.Serialize qryTweet) |> ignore
        result.ToString()
    
    let Start(): StatefulAgent<S2CMessage, C2SMessage, int> =
        fun client -> async{
            return 0, fun state msg -> async {
                match msg with
                | Message data ->
                    match data with
                    | LoginVerify loginInfo ->
                        let loginInfo: UsernameToken = Json.Deserialize loginInfo
                        if checkLogin(loginInfo.username, loginInfo.token) = false then
                            do! client.PostAsync(LoginVerifyResult false)
                        else
                            if tokenClient.ContainsKey(loginInfo.token) = false then
                                tokenClient.Add(loginInfo.token, client)
                            else if tokenClient.[loginInfo.token] <> client then
                                tokenClient.Remove(loginInfo.token) |> ignore
                                tokenClient.Add(loginInfo.token, client)
                    | Logout logoutInfo ->
                        let logoutInfo: UsernameToken = Json.Deserialize logoutInfo
                        usernameToken.Remove(logoutInfo.username) |> ignore
                        tokenClient.Remove(logoutInfo.token) |> ignore
                    | FollowOperation followInfo ->
                        let followInfo: FollowInfo = Json.Deserialize followInfo
                        if followInfo.username.Length > 0 then
                            let checkAccount = DBOperator.accountDAO.getAccountByUsername(followInfo.username)
                            if checkAccount.ID <> -1 then
                                let newFollow = new Follow(followInfo.username, followInfo.follower)
                                if DBOperator.followDAO.insert(newFollow) = false then
                                    do! client.PostAsync(FollowResult false)
                                else
                                    do! client.PostAsync(FollowResult true)
                            else
                                do! client.PostAsync(FollowResult false)
                    | TweetContent tweetContent ->
                        let mutable processSign = true
                        let tweetInfo: TweetInfo = Json.Deserialize tweetContent
                        if tweetInfo.retweetID <> -1 then
                            let verifyTweet = DBOperator.tweetDAO.getTweetByTweetID(tweetInfo.retweetID)
                            if verifyTweet.ID = -1 then
                                processSign <- false
                        if processSign then
                            // split by "//" in tweet content text
                            // First part Content; Second part Mention(@); Third part Hashtag(#)
                            // In Second part, each mention start with "@" split by ";"
                            // In third part, each hashtag start with "#" split by ";
                            let mutable content = tweetInfo.content
                            while content.IndexOf("\n") <> -1 do
                                content <- content.Replace("\n", "")
                            let parts = content.Split("//")
                            let mutable mentions = new List<string>()
                            let mutable hashtags = new List<string>()
                            if parts.Length >= 2 then
                                if parts.[1].IndexOf("@") <> -1 then
                                    let rawMentions = parts.[1].Split(";")
                                    for i in 0 .. rawMentions.Length - 1 do
                                        mentions.Add(rawMentions.[i].Replace("@", ""))
                                else
                                    let rawHashtags = parts.[1].Split(";")
                                    for i in 0 .. rawHashtags.Length - 1 do
                                        hashtags.Add(rawHashtags.[i].Replace("#", ""))
                            if parts.Length >= 3 then
                                let rawHashtags = parts.[2].Split(";")
                                for i in 0 .. rawHashtags.Length - 1 do
                                    hashtags.Add(rawHashtags.[i].Replace("#", ""))
                            let newTweet = new Tweet(
                                                tweetInfo.creator,
                                                parts.[0],
                                                tweetInfo.retweetID
                                            )
                            DBOperator.tweetDAO.insert(newTweet) |> ignore
                            let tweetID = DBOperator.tweetDAO.getLastInsertRowID()
                            for hashtag in hashtags do
                                let newHashtag = new Hashtag(hashtag, tweetInfo.creator)
                                DBOperator.hashtagDAO.insert(newHashtag) |> ignore
                                let hashtagID = DBOperator.hashtagDAO.getLastInsertRowID()
                                let newTweetHashtag = new TweetHashtag(tweetID, hashtagID)
                                DBOperator.tweetHashtagDAO.insert(newTweetHashtag) |> ignore
                            for mention in mentions do
                                let accountName = DBOperator.accountDAO.getAccountByUsername(mention)
                                if accountName.ID <> -1 then
                                    let newTweetMention = new TweetMention(tweetID, mention)
                                    DBOperator.tweetMentionDAO.insert(newTweetMention) |> ignore
                            
                            let followingNewTweet: FollowingNewTweetInfo = {
                                id = tweetID
                                creator = tweetInfo.creator
                                content = tweetInfo.content
                                retweetID = tweetInfo.retweetID
                            }
                            let followers = DBOperator.followDAO.getFollowersByUsername(tweetInfo.creator)
                            for follower in followers do
                                if usernameToken.ContainsKey(follower) then
                                    let token = usernameToken.[follower]
                                    if tokenClient.ContainsKey(token) then
                                        let client = tokenClient.[usernameToken.[follower]]
                                        do! client.PostAsync(FollowingNewTweet (Json.Serialize followingNewTweet))
                    | QryFollowingName name ->
                        let tweets = DBOperator.tweetDAO.getTweetsByCreator(name)
                        let result = buildTweetsString(tweets)
                        do! client.PostAsync(QryFollowingNameTweet result)
                    | QryHashtag topic ->
                        let hashtag = DBOperator.hashtagDAO.getHashtagByTopic(topic)
                        let tweetIDs = DBOperator.tweetHashtagDAO.getTweetIDsByHashtagID(hashtag.ID)
                        let tweets = DBOperator.tweetDAO.getTweetsByTweetIDs(tweetIDs)
                        let result = buildTweetsString(tweets)
                        if result.Length > 0 then
                            do! client.PostAsync(QryHashtagTweet result)
                    | QryMention mention ->
                        let tweetIDs = DBOperator.tweetMentionDAO.getTweetIDsByUsername(mention)
                        let tweets = DBOperator.tweetDAO.getTweetsByTweetIDs(tweetIDs)
                        let result = buildTweetsString(tweets)
                        if result.Length > 0 then
                            do! client.PostAsync(QryMentionTweet result)
                    return state + 1
                | Error error ->
                    printfn "%A" error
                    return state
                | Close ->
                    printfn "Close"
                    return state
            }
        }
