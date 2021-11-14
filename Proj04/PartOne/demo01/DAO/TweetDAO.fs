namespace DAO

open System.Collections.Generic
open System.Data.SQLite

open Entities

type TweetDAO(connection: SQLiteConnection) =
    
    member this.insert(tweet: Tweet): bool =
        let sql = "insert into Tweet (Creator, Content, RetweetID) values(@creator, @content, @retweetID)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@creator", tweet.CREATOR) |> ignore
        command.Parameters.AddWithValue("@content", tweet.CONTENT) |> ignore
        command.Parameters.AddWithValue("@retweetID", tweet.RETWEETID) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException -> 
            false

    member this.getTweetsByCreator(creator: string): List<Tweet> =
        let sql = "select * from Tweet where creator = @creator"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@creator", creator) |> ignore
        let tweets = new List<Tweet>()
        let reader = command.ExecuteReader()
        while reader.Read() do
            let tweet = new Tweet(
                            reader.["CREATOR"].ToString(),
                            reader.["CONTENT"].ToString(),
                            reader.["RETWEETID"].ToString() |> int
                        )
            tweets.Add(tweet)
        tweets