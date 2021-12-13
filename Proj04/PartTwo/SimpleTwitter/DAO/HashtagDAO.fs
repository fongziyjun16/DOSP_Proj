namespace SimpleTwitter

open System.Data.SQLite

type HashtagDAO(connection: SQLiteConnection) =
    
    member this.insert(hashtag: Hashtag): bool =
        let sql = "insert into Hashtag(topic, creator) values(@topic, @creator)"
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
