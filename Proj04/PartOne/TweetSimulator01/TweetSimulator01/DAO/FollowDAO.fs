namespace DAO

open System.Collections.Generic
open System.Data.SQLite

open Entities

type FollowDAO(connection: SQLiteConnection) =
    
    member this.insert(follow: Follow): bool = 
        let sql = "insert into Follow values(@name, @follower)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@name", follow.NAME) |> ignore
        command.Parameters.AddWithValue("@follower", follow.FOLLOWER) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getFollowersByName(name: string): List<string> =
        let sql = "select * from follow where name = @name"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@name", name) |> ignore
        let followers = new List<string>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            followers.Add(reader.["FOLLOWER"].ToString())
        followers

    member this.getFollowsByName(follower: string): List<string> =
        let sql = "select * from follow where follower = @follower"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@follower", follower) |> ignore
        let follows = new List<string>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            follows.Add(reader.["NAME"].ToString())
        follows
    
    member this.getFollowersNumberOfClient(name: string): int =
        let sql = "select count(*) as number from follow where name = @name"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@name", name) |> ignore
        use reader = command.ExecuteReader()
        if reader.Read() then
            reader.["number"].ToString() |> int
        else
            0

