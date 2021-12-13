namespace SimpleTwitter

open WebSharper
open WebSharper.JavaScript

[<NamedUnionCases>]
type C2SMessage =
    | LoginVerify of loginInfo: string
    | Logout of logoutInfo: string
    | FollowOperation of followInfo: string
    | TweetContent of content: string
    | QryFollowingName of name: string
    | QryHashtag of hashtag: string
    | QryMention of mention: string
    
[<NamedUnionCases>]
type S2CMessage =
    | LoginVerifyResult of value: bool
    | FollowResult of followResult: bool
    | FollowingNewTweet of followingNewTweet: string
    | QryFollowingNameTweet of followingTweet: string
    | QryHashtagTweet of hashtagTweet: string
    | QryMentionTweet of mentionTweet: string

type LoginVerification = {
    loginSign: bool
    token: string
}

[<JavaScript>]
type UsernameToken = {
    username: string
    token: string
}

[<JavaScript>]
type FollowInfo = {
    username: string
    follower: string
}

[<JavaScript>]
type TweetInfo = {
    creator: string
    content: string
    retweetID: int
}

[<JavaScript>]
type FollowingNewTweetInfo = {
    id: int
    creator: string
    content: string
    retweetID: int
}

