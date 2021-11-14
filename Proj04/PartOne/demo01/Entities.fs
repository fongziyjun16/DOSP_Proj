module Entities

type Account = 
    struct
        val USERNAME: string
        val PASSWORD: string
        val NAME: string
        val GENDER: string
        new (username: string, password: string, name: string, gender: string) = {
            USERNAME = username;
            PASSWORD = password;
            NAME = name;
            GENDER = gender;
        }
    end

type Follow = 
    struct
        val USERNAME: string
        val FOLLOWER: string
        new (username: string, follower: string) = {
            USERNAME = username;
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

    
type Mention = 
    struct
        val TWEETID: int
        val NAME: string // account name
        new (tweetID: int, name: string) = {
            TWEETID = tweetID;
            NAME = name;
        }
    end

type Tweet = 
    struct
        val ID: int
        val CREATOR: string // account username
        val CONTENT: string 
        val RETWEETID: int
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


type TweetHashtag = 
    struct
        val TWEETID: int
        val HASHTAGID: int
        new (tweetID: int, hashtagID: int) = {
            TWEETID = tweetID;
            HASHTAGID = hashtagID;
        }
    end

