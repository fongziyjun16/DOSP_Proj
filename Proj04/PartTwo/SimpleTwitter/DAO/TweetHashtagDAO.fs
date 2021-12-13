namespace SimpleTwitter

open System.Collections.Generic
open System.Data.SQLite

type TweetHashtagDAO(connection: SQLiteConnection) =
    
    member this.insert(tweetHashtag: TweetHashtag): bool =
        let sql = "insert into Tweet_Hashtag values(@tweetID, @hashtagID)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@tweetID", tweetHashtag.TWEETID) |> ignore
        command.Parameters.AddWithValue("@hashtagID", tweetHashtag.HASHTAGID) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getTweetIDsByHashtagID(hashtagID: int): List<int> =
        let sql = "select * from Tweet_Hashtag where hashtagID = @hashtagID"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@hashtagID", hashtagID.ToString()) |> ignore
        use reader = command.ExecuteReader()
        let tweetIDs = new List<int>()
        while reader.Read() do
            tweetIDs.Add(reader.["TWEETID"].ToString() |> int) |> ignore
        tweetIDs

