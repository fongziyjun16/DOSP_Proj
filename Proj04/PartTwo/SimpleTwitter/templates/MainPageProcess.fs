namespace SimpleTwitter

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client

[<JavaScript>]
module MainPageProcess =
    
    let Process(ep: WebSocketEndpoint<S2CMessage, C2SMessage>) =
        
        let mutable realTimeServer: WebSocketServer<S2CMessage,C2SMessage> option = None
        
        let getCookie(key: string): string =
            let cookiesString = new String(JS.Document.Cookie)
            let cookies = cookiesString.Split(";")
            match key with
            | "username" ->
                cookies.[0].Substring(cookies.[0].IndexOf("=") + 1)
            | "token" ->
                cookies.[1].Substring(cookies.[1].IndexOf("=") + 1)
            | _ ->
                ""
    
        let logoutOperation() =
            JS.Document.Cookie <- "username="
            JS.Document.Cookie <- "token="
            let mutable accountURL = new String(JS.Window.Location.Href)
            accountURL <- String(accountURL.Substring(0, accountURL.LastIndexOf("/")))
            JS.Window.Location.Replace(accountURL.ToString())
    
        let loginVerify() =
            let loginInfoObj: UsernameToken = {
                username = getCookie("username");
                token = getCookie("token");
            }
            realTimeServer.Value.Post(LoginVerify (Json.Stringify loginInfoObj))

        let setElementInnerHTML(eleID: string, content: string) =
            JS.Document.GetElementById(eleID).InnerHTML <- content
        
        let addLi2Table(tableID: string, tweetInfo: string, clearFirst: bool) =
            let ul = JS.Document.GetElementById(tableID)
            if clearFirst then
                ul.InnerHTML <- ""
            let li = JS.Document.CreateElement("li")
            li.AppendChild(JS.Document.CreateTextNode(tweetInfo)) |> ignore
            li.SetAttribute("class", "list-group-item")
            ul.AppendChild(li) |> ignore
        
        let addTweets2Table(qryTweets: string, tableID: string) =
            addLi2Table(tableID, "", true)
            let tweets = String(qryTweets).Split("}{")
            // Console.Log(tweets)
            for tweet in tweets do
                addLi2Table(tableID, tweet, false)
        
        let wsConnect =
            async {
                return! ConnectStateful ep <| fun wsConnect -> async {
                    return 0, fun state msg -> async {
                        match msg with
                        | Open ->
                            Console.Log("websocket open")
                            return state
                        | Message data ->
                            match data with
                            | LoginVerifyResult result ->
                                if result = false then
                                    logoutOperation()
                            | FollowResult result ->
                                if result = false then
                                    setElementInnerHTML(
                                        "followState",
                                        "Follow Fail. Following not exist or has been followed"
                                    )
                                else
                                    setElementInnerHTML(
                                        "followState",
                                        "Follow Success"
                                    )
                            | FollowingNewTweet followingNewTweet ->
                                // Console.Log(followingNewTweet)
                                addLi2Table("realTimeTweetTable", followingNewTweet, false)
                            | QryFollowingNameTweet followingNameTweet ->
                                addTweets2Table(followingNameTweet, "followingTweetTable")
                            | QryHashtagTweet qryHashtagTweet ->
                                addTweets2Table(qryHashtagTweet, "qryHashtagTweetTable")
                            | QryMentionTweet qryMentionTweet ->
                                addTweets2Table(qryMentionTweet, "qryMentionTable")
                            return state + 1
                        | Close ->
                            Console.Log("websocket close")
                            logoutOperation()
                            return state
                        | Error ->
                            logoutOperation()
                            return state
                    }
                }
            }
        wsConnect.AsPromise().Then(fun x -> realTimeServer <- Some(x)) |> ignore
        
        let username = Var.Create ""
        username.Value <- getCookie("username")
        Templates.MainTemplate.Main()
            .Username(username.View)
            .Logout(fun e ->
                async {
                    logoutOperation()
                    // send username token to RealTimeServer to logout
                    let usernameToken: UsernameToken = {
                                username = getCookie("username");
                                token = getCookie("token");
                            }
                    Console.Log(realTimeServer.Value)
                    realTimeServer.Value.Post(Logout (Json.Stringify usernameToken))
                }
                |> Async.StartImmediate
            )
            .toFollow(fun e ->
                async {
                    loginVerify()
                    let followingUsername = e.Vars.following.Value
                    if followingUsername.Length > 0 then
                        let followInfo: FollowInfo = {
                            username = followingUsername
                            follower = getCookie("username")
                        }
                        realTimeServer.Value.Post(FollowOperation (Json.Stringify followInfo))
                        e.Vars.following.Value <- ""
                    else
                        setElementInnerHTML(
                            "followState",
                            "Follow Input has been REQUIRED"
                        )
                }
                |> Async.StartImmediate
            )
            .postTweet(fun e ->
                async {
                    let mutable content = String(e.Vars.tweetContent.Value)
                    if content.Length > 0 then
                        let tweetInfo: TweetInfo = {
                            creator = getCookie("username")
                            content = content.ToString()
                            retweetID = -1
                        }
                        realTimeServer.Value.Post(TweetContent (Json.Stringify tweetInfo))
                }
                |> Async.StartImmediate
            )
            .retweet(fun e ->
                async {
                    let mutable content = String(e.Vars.retweetContent.Value)
                    if content.Length > 0 then
                        let tweetInfo: TweetInfo = {
                            creator = getCookie("username")
                            content = content.ToString()
                            retweetID = e.Vars.retweetID.Value |> int
                        }
                        realTimeServer.Value.Post(TweetContent (Json.Stringify tweetInfo))
                }
                |> Async.StartImmediate
            )
            .qryTweetFromFollowing(fun e ->
                async {
                    let mutable followingName = String(e.Vars.followingName.Value)
                    if followingName.Length > 0 then
                        realTimeServer.Value.Post(QryFollowingName (followingName.ToString()))
                }
                |> Async.StartImmediate
            )
            .qryTweetFromHashtag(fun e ->
                async {
                    let mutable hashtag = String(e.Vars.hahstagName.Value)
                    if hashtag.Length > 0 then
                        realTimeServer.Value.Post(QryHashtag (hashtag.ToString()))
                }
                |> Async.StartImmediate
            )
            .qryTweetFromMention(fun e ->
                async {
                    let mutable username = getCookie("username")
                    realTimeServer.Value.Post(QryMention username)
                }
                |> Async.StartImmediate
            )
            .Doc()
        
