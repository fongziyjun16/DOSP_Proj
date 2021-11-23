namespace Msgs

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

type LoginInfo = 
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type LogoutInfo =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type SubscribeInfo = 
    struct
        val FOLLOW: string
        val FOLLOWER: string
        new (follow: string, follower: string) = {
            FOLLOW = follow;
            FOLLOWER = follower;
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

type QueryFollowInfo =
    struct
        val FOLLOWER: string
        new (follower: string) = {
            FOLLOWER = follower;
        }
    end

type QueryMentionInfo =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type QueryHashtagsInfo =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end
    

