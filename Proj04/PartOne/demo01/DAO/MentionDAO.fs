namespace DAO

open System.Collections.Generic
open System.Data.SQLite

open Entities

type MentionDAO(connection: SQLiteConnection) =

    member this.insert(mention: Mention): bool =
        let sql = "insert into Mention values(@tweetID, @name)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@tweetID", mention.TWEETID) |> ignore
        command.Parameters.AddWithValue("@name", mention.NAME) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getMentionsByTweetID(tweetID: int): List<string> =
        let sql = "select * from Mention where tweetID = @tweetID"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@tweetID", tweetID) |> ignore
        let mentions = new List<string>()
        let reader = command.ExecuteReader()
        while reader.Read() do
            mentions.Add(reader.["NAME"].ToString())
        mentions