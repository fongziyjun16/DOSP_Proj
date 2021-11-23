namespace Msgs

open System.Collections.Generic

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

type TweetDTO =
    struct
        val ID: int
        val CREATOR: string
        val CONTENT: string
        val MENTIONS: List<string>
        val HASHTAGS: List<string>
        val RETWEET: bool
        new (id: int, creator: string, content: string, mentions: List<string>, hashtags: List<string>, retweet: bool) = {
            ID = id;
            CREATOR = creator;
            CONTENT = content;
            MENTIONS = mentions;
            HASHTAGS = hashtags;
            RETWEET = retweet;
        }

        member this.getID(): int =
            this.ID

        member this.toString(): string =
            Tools.buildPrintingTweet(this.CREATOR, this.CONTENT, this.MENTIONS, this.HASHTAGS, this.RETWEET)
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
        val TWEETS: List<TweetDTO>
        new (tweets: List<TweetDTO>) = {
            TWEETS = tweets;
        }
    end

type QueryMentionOperation = 
    struct
    end

type QueryMentionResult =
    struct
        val TWEETS: List<TweetDTO>
        new (tweets: List<TweetDTO>) = {
            TWEETS = tweets;
        }
    end

type QueryHashtagsOperation = 
    struct
    end

type QueryHashtagsResult =
    struct
        val TWEETS: List<TweetDTO>
        new (tweets: List<TweetDTO>) = {
            TWEETS = tweets;
        }
    end

