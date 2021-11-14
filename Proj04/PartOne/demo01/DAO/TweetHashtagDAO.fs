namespace DAO

open System.Collections.Generic
open System.Data.SQLite

open Entities

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

    member this.getHashtagIDByTweetID(tweetID: int): List<int> =
        let sql = "select * from Tweet_Hashtag where tweetID = @tweetID"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@tweetID", tweetID) |> ignore
        let reader = command.ExecuteReader()
        let hashtagIDs = new List<int>()
        while reader.Read() do
            hashtagIDs.Add(reader.["HASHTAGID"].ToString() |> int)
        hashtagIDs

