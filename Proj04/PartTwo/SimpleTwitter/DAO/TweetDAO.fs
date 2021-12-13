namespace SimpleTwitter

open System.Text
open System.Collections.Generic
open System.Data.SQLite

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
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg = false then 1
        else reader.["last_rowid"].ToString() |> int

    member this.getTweetByTweetID(tweetID: int): Tweet =
        let sql = "select * from tweet where id = @id"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@id", tweetID) |> ignore
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg = false then new Tweet(-1, "", "", -1)
        else
            new Tweet(
                reader.["ID"].ToString() |> int,
                reader.["CREATOR"].ToString(),
                reader.["CONTENT"].ToString(),
                reader.["RETWEETID"].ToString() |> int
            )
            
    member this.getTweetsByCreator(creator: string): List<Tweet> =
        let sql = "select * from Tweet where creator = @creator"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@creator", creator) |> ignore
        let tweets = new List<Tweet>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            let tweet = new Tweet(
                            reader.["CREATOR"].ToString(),
                            reader.["CONTENT"].ToString(),
                            reader.["RETWEETID"].ToString() |> int
                        )
            tweets.Add(tweet)
        tweets

    member this.getTweetsByTweetIDs(tweetIDs: List<int>): List<Tweet> = 
        let sql = new StringBuilder("select * from Tweet where ID in (")
        let numberOfTweetIDs = tweetIDs.Count
        for i in 1 .. numberOfTweetIDs do
            if i < numberOfTweetIDs then
                sql.Append("@tweetID" + i.ToString() + ", ") |> ignore
            else
                sql.Append("@tweetID" + i.ToString()) |> ignore
        sql.Append(")") |> ignore

        use command = new SQLiteCommand(sql.ToString(), connection)
        for i in 1 .. numberOfTweetIDs do
            command.Parameters.AddWithValue("@tweetID" + i.ToString(), tweetIDs.[i - 1]) |> ignore
        let tweets = new List<Tweet>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            let tweet = new Tweet(
                            reader.["ID"].ToString() |> int,
                            reader.["CREATOR"].ToString(),
                            reader.["CONTENT"].ToString(),
                            reader.["RETWEETID"].ToString() |> int
                        )
            tweets.Add(tweet)
        tweets
