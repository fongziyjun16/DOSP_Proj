module Entities

open System.Text

type Account = 
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type Follow = 
    struct
        val NAME: string
        val FOLLOWER: string
        new (name: string, follower: string) = {
            NAME = name;
            FOLLOWER = follower;
        }
    end

type Hashtag = 
    struct
        val ID: int
        val TOPIC: string
        val CREATOR: string // account username
        new (topic: string, creator: string) = {
            ID = -1;
            TOPIC = topic;
            CREATOR = creator;
        }
        new (id: int, topic: string, creator: string) = {
            ID = id;
            TOPIC = topic;
            CREATOR = creator;
        }
    end

type Tweet = 
    struct
        val ID: int
        val CREATOR: string // account name
        val CONTENT: string 
        val RETWEETID: int
        new (creator: string, content: string) = {
            ID = -1;
            CREATOR = creator;
            CONTENT = content;
            RETWEETID = -1;
        }
        new (creator: string, content: string, retweetID: int) = {
            ID = -1;
            CREATOR = creator;
            CONTENT = content;
            RETWEETID = retweetID;
        }
        new (id: int, creator: string, content: string, retweetID: int) = {
            ID = id;
            CREATOR = creator;
            CONTENT = content;
            RETWEETID = retweetID;
        }
    end

type TweetMention = 
    struct
        val TWEETID: int
        val NAME: string // account name
        new (tweetID: int, name: string) = {
            TWEETID = tweetID;
            NAME = name;
        }
    end

type TweetHashtag = 
    struct
        val TWEETID: int
        val HASHTAGID: int
        new (tweetID: int, hashtagID: int) = {
            TWEETID = tweetID;
            HASHTAGID = hashtagID;
        }
    end

