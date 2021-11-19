module Msgs

open System.Collections.Generic

open Entities

type RegisterInfo = 
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type RegisterSuccessInfo =
    struct
    end

type RegisterFailureInfo =
    struct
    end

type RegisterOperationInfo =
    struct
    end

type LoginOperation =
    struct
    end

type LoginInfo = 
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type LogoutOperation =
    struct
    end

type LogoutInfo =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type SubscribeOperation =
    struct
    end

type SubscribeInfo = 
    struct
        val FOLLOWER: string
        new (follower: string) = {
            FOLLOWER = follower;
        }
    end

type PostTweetOperation =
    struct
        val RETWEETFLAG: bool
        new (retweetFlag: bool) = {
            RETWEETFLAG = retweetFlag;
        }
    end

type PostTweetInfo = 
    struct
        val NAME: string
        val CONTENT: string
        val NUMBEROFMENTIONS: int
        val NUMBEROFEXISTINGHASHTAGS: int
        val HASHTAGS: List<string>
        val RETWEETFLAG: bool
        new (name: string, content: string, numberOfMentions: int, numberOfExistingHashtags: int, hashtags: List<string>, retweetFlag: bool) = {
            NAME = name;
            CONTENT = content;
            NUMBEROFMENTIONS = numberOfMentions;
            NUMBEROFEXISTINGHASHTAGS = numberOfExistingHashtags;
            HASHTAGS = hashtags;
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

type QueryFollowInfo =
    struct
        val FOLLOWER: string
        new (follower: string) = {
            FOLLOWER = follower;
        }
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

type QueryMentionInfo =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
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

type QueryHashtagsInfo =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type QueryHashtagsResult =
    struct
        val TWEETS: List<Tweet>
        new (tweets: List<Tweet>) = {
            TWEETS = tweets;
        }
    end