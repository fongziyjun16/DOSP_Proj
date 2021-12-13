namespace SimpleTwitter

open System.Collections.Generic
open System.Data.SQLite

type TweetMentionDAO(connection: SQLiteConnection) =

    member this.insert(mention: TweetMention): bool =
        let sql = "insert into Tweet_Mention values(@tweetID, @username)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@tweetID", mention.TWEETID) |> ignore
        command.Parameters.AddWithValue("@username", mention.USERNAME) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException as e ->
            false

    member this.getTweetIDsByUsername(username: string): List<int> =
        let sql = "select * from Tweet_Mention where username = @username"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@username", username) |> ignore
        let mentions = new List<int>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            mentions.Add(reader.["TWEETID"].ToString() |> int)
        mentions