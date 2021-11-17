namespace DAO

open System.Collections.Generic
open System.Data.SQLite
open System.Text

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

    member this.getLastInsertRowID(): int =
        let sql = "select last_insert_rowid() as last_rowid from tweet"
        use command = new SQLiteCommand(sql, connection)
        let reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg = false then 1
        else reader.["last_rowid"].ToString() |> int

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

    member this.updateTweetRetweetIDByTweetID(tweetID: int, retweetID: int): bool =
        let sql = "update Tweet set retweetID = @retweetID where ID = @tweetID"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@retweetID", retweetID) |> ignore
        command.Parameters.AddWithValue("@tweetID", tweetID) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getTweetsByCreators(follows: List<string>): List<Tweet> =
        let sql = new StringBuilder("select * from Tweet where creator in (") 
        let numberOfFollows = follows.Count
        for i in 1 .. numberOfFollows do
            if i < numberOfFollows then
                sql.Append("@create" + i.ToString() + ", ") |> ignore
            else 
                sql.Append("@create" + i.ToString() + ")") |> ignore
        sql.Append(" and rowid < 31") |> ignore
        use command = new SQLiteCommand(sql.ToString(), connection)
        for i in 1 .. numberOfFollows do
            command.Parameters.AddWithValue("@create" + i.ToString(), follows.[i - 1]) |> ignore
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

