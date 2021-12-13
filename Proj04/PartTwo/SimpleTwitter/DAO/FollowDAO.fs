namespace SimpleTwitter

open System.Collections.Generic
open System.Data.SQLite

type FollowDAO(connection: SQLiteConnection) =
    
    member this.insert(follow: Follow): bool = 
        let sql = "insert into Follow values(@username, @follower)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@username", follow.USERNAME) |> ignore
        command.Parameters.AddWithValue("@follower", follow.FOLLOWER) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getFollowersByUsername(username: string): List<string> =
        let sql = "select * from follow where username = @username"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@username", username) |> ignore
        let followers = new List<string>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            followers.Add(reader.["FOLLOWER"].ToString())
        followers