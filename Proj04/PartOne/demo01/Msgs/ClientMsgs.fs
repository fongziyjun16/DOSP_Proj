namespace Msgs

open System.Collections.Generic

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
        val NAME: string
        val CONTENT: string
        val RETWEETID: int
        new (name: string, content: string, retweetID: int) = {
            NAME = name;
            CONTENT = content;
            RETWEETID = retweetID;
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
        val TWEETS: List<Tweet>
        new (tweets: List<Tweet>) = {
            TWEETS = tweets;
        }
    end

type QueryMentionOperation = 
    struct
    end

type QueryMentionResult =
    struct
        val TWEETS: List<Tweet>
        new (tweets: List<Tweet>) = {
            TWEETS = tweets;
        }
    end

type QueryHashtagsOperation = 
    struct
    end

type QueryHashtagsResult =
    struct
        val TWEETS: List<Tweet>
        new (tweets: List<Tweet>) = {
            TWEETS = tweets;
        }
    end

