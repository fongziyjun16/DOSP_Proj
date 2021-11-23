namespace DAO

open System.Collections.Generic
open System.Data.SQLite

open Entities

type TweetMentionDAO(connection: SQLiteConnection) =

    member this.insert(mention: TweetMention): bool =
        let sql = "insert into Tweet_Mention values(@tweetID, @name)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@tweetID", mention.TWEETID) |> ignore
        command.Parameters.AddWithValue("@name", mention.NAME) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException as e ->
            false

    member this.getMentionsByTweetID(tweetID: int): List<string> =
        let sql = "select * from Tweet_Mention where tweetID = @tweetID"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@tweetID", tweetID) |> ignore
        let mentions = new List<string>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            mentions.Add(reader.["NAME"].ToString())
        mentions

    member this.getTweetIDsByName(name: string): List<int> =
        let sql = "select * from Tweet_Mention where name = @name"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@name", name) |> ignore
        let mentions = new List<int>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            mentions.Add(reader.["TWEETID"].ToString() |> int)
        mentions