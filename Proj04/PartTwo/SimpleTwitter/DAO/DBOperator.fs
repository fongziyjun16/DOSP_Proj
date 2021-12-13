namespace SimpleTwitter

open System.Data.SQLite

module DBOperator =
    
    let getNewOpenDBConnection(): SQLiteConnection = 
        let newConnection = new SQLiteConnection("Data Source=./resources/twitter_sys.db;Version=3;")
        newConnection.Open()
        newConnection

    let accountDAO = new AccountDAO(getNewOpenDBConnection())
    let followDAO = new FollowDAO(getNewOpenDBConnection())
    let hashtagDAO = new HashtagDAO(getNewOpenDBConnection())
    let tweetDAO = new TweetDAO(getNewOpenDBConnection())
    let tweetMentionDAO = new TweetMentionDAO(getNewOpenDBConnection())
    let tweetHashtagDAO = new TweetHashtagDAO(getNewOpenDBConnection())

