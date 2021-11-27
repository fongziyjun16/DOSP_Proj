namespace DAO

open System.Text
open System.Data.SQLite
open System.Collections.Generic

open Entities

type HashtagDAO(connection: SQLiteConnection) =
    
    member this.insert(hashtag: Hashtag): bool =
        let sql = "insert into Hashtag(TOPIC, CREATOR) values(@topic, @creator)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@topic", hashtag.TOPIC) |> ignore
        command.Parameters.AddWithValue("@creator", hashtag.CREATOR) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getLastInsertRowID(): int =
        let sql = "select last_insert_rowid() as last_rowid from hashtag"
        use command = new SQLiteCommand(sql, connection)
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg = false then 1
        else reader.["last_rowid"].ToString() |> int

    member this.getHashtagByTopic(topic: string): Hashtag =
        let sql = "select * from Hashtag where topic = @topic"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@topic", topic) |> ignore
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg then
            new Hashtag(
                reader.["ID"].ToString() |> int,
                reader.["TOPIC"].ToString(),
                reader.["CREATOR"].ToString()
            )
        else
            new Hashtag(-1, "", "")

    member this.getTopicByHashtagID(id: int): string =
        let sql = "select * from Hashtag where id = @id"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@id", id) |> ignore
        use reader = command.ExecuteReader()
        let read = reader.Read()
        if read then
            reader.["TOPIC"].ToString()
        else
            ""
    
    member this.getTopicsByHashtagIDs(hashtagIDs: List<int>): List<string> =
        let sql = new StringBuilder("select * from Hashtag where id in (")
        for i in 1 .. hashtagIDs.Count do
            sql.Append("@id" + i.ToString()) |> ignore
        sql.Append(")") |> ignore
        use command = new SQLiteCommand(sql.ToString(), connection)
        for i in 1 .. hashtagIDs.Count do
            command.Parameters.AddWithValue("@id" + i.ToString(), hashtagIDs.[i - 1]) |> ignore
        let topics = new List<string>()
        use reader = command.ExecuteReader()
        while reader.Read() do
            topics.Add(reader.["topic"].ToString()) |> ignore 
        topics
            
        
        
    