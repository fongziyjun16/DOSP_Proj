namespace Msgs

open System.Collections.Generic

open System.Text
open ToolsKit
open Entities

type RegisterOperation =
    struct
    end

type LoginOperation =
    struct
    end

type LogoutOperation =
    struct
    end

type SubscribeOperation =
    struct
        val FOLLOW: string
        new (follow: string) = {
            FOLLOW = follow;
        }
    end

type SetIndex =
    struct
        val INDEX: int
        new (index: int) = {
            INDEX = index
        }
    end

type SimpleTweetDTO =
    struct
        val TWEETID: int
        val RETWEETID: int
        new (tweetID: int, retweetID: int) = {
            TWEETID = tweetID;
            RETWEETID = retweetID;
        }
        
        member this.toString(): string =
            let sb = new StringBuilder()
            sb.Append("{ ") |> ignore
            sb.Append(this.TWEETID) |> ignore
            if this.RETWEETID <> -1 then
                sb.Append("(" + string(this.RETWEETID) + ")") |> ignore
            sb.Append(" }") |> ignore
            sb.ToString()
    end

type TweetDTO =
    struct
        val ID: int
        val CREATOR: string
        val CONTENT: string
        val MENTIONS: List<string>
        val HASHTAGS: List<string>
        val RETWEETID: int
        new (id: int, creator: string, content: string, mentions: List<string>, hashtags: List<string>, retweetID: int) = {
            ID = id;
            CREATOR = creator;
            CONTENT = content;
            MENTIONS = mentions;
            HASHTAGS = hashtags;
            RETWEETID = retweetID;
        }

        member this.getID(): int =
            this.ID

        member this.toString(): string =
            Tools.buildPrintingTweet(this.CREATOR, this.CONTENT, this.MENTIONS, this.HASHTAGS, this.RETWEETID)
    
        member this.toSimpleString(): string =
            let mutable baseString = new StringBuilder("{ " + this.ID.ToString())
            if this.RETWEETID <> -1 then
                baseString.Append("(" + this.RETWEETID.ToString() + ")") |> ignore
            baseString.Append(" }") |> ignore
            baseString.ToString()
    end

type PostTweetOperation =
    struct
        val RETWEETFLAG: bool
        new (retweetFlag: bool) = {
            RETWEETFLAG = retweetFlag;
        }
    end

type DeliverTweetOperation = 
    struct
        val TWEET: TweetDTO
        new (tweet: TweetDTO) = {
            TWEET = tweet;
        }
    end

type DeliverTweetsOperation = 
    struct
        val TWEETS: List<Tweet>
        new (tweets: List<Tweet>) = {
            TWEETS = tweets;
        }
    end

type QueryFollowOperation = 
    struct
    end
    
type QueryFollowResult =
    struct
        val TWEETS: List<SimpleTweetDTO>
        new (tweets: List<SimpleTweetDTO>) = {
            TWEETS = tweets;
        }
    end

type QueryMentionOperation = 
    struct
    end

type QueryMentionResult =
    struct
        val TWEETS: List<SimpleTweetDTO>
        new (tweets: List<SimpleTweetDTO>) = {
            TWEETS = tweets;
        }
    end

type QueryHashtagOperation = 
    struct
    end

type QueryHashtagResult =
    struct
        val HASHTAG: string
        val TWEETS: List<SimpleTweetDTO>
        new (hashtag: string, tweets: List<SimpleTweetDTO>) = {
            HASHTAG = hashtag;
            TWEETS = tweets;
        }
    end

type SimulationOperation =
    struct
    end
    
type StopSimulationOperation =
    struct
    end
