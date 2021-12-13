namespace SimpleTwitter

open System.Data.SQLite

type AccountDAO(connection: SQLiteConnection) =
    
    member this.insert(account: Account): bool =
        let sql = "insert into account(username, password) values(@username, @password)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@username", account.USERNAME) |> ignore
        command.Parameters.AddWithValue("@password", account.PASSWORD) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getAccountByUsername(username: string): Account =
        let sql = "select * from account where username = @username"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@username", username) |> ignore
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg then
            new Account(
                reader.["ID"].ToString() |> int,
                reader.["USERNAME"].ToString(),
                reader.["PASSWORD"].ToString()
            )
        else
            new Account(-1, "", "")
            
    member this.getLastInsertRowID(): int =
        let sql = "select last_insert_rowid() as last_rowid from account"
        use command = new SQLiteCommand(sql, connection)
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg = false then 1
        else reader.["last_rowid"].ToString() |> int
        
    member this.getAccountNameByID(id: int): string =
        let sql = "select * from account where id = @id"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@id", id) |> ignore
        use reader = command.ExecuteReader()
        let read = reader.Read()
        if read then
            reader.["USERNAME"].ToString()
        else
            ""