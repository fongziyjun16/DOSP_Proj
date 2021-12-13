namespace SimpleTwitter

type Account = 
    struct
        val ID: int
        val USERNAME: string
        val PASSWORD: string
        new (username: string, password: string) = {
            ID = -1;
            USERNAME = username
            PASSWORD = password
        }
        new (id: int, username: string, password: string) = {
            ID = id
            USERNAME = username
            PASSWORD = password
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

type Tweet = 
    struct
        val ID: int
        val CREATOR: string // account username
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
        val USERNAME: string // account username
        new (tweetID: int, username: string) = {
            TWEETID = tweetID;
            USERNAME = username;
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


