namespace DAO

open System.Data.SQLite

open Entities

type AccountDAO(connection: SQLiteConnection) =

    member this.insert(account: Account): bool =
        let sql = "insert into account(name) values(@name)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@name", account.NAME) |> ignore
        try
            command.ExecuteNonQuery() |> ignore
            true
        with
        | :? SQLiteException ->
            false

    member this.getAccountByUsername(name: string): Account =
        let sql = "select * from account where name = @name"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@name", name) |> ignore
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg then
            new Account(
                reader.["ID"].ToString() |> int,
                reader.["NAME"].ToString()
            )
        else
            new Account(-1, "")

    member this.getLastInsertRowID(): int =
        let sql = "select last_insert_rowid() as last_rowid from account"
        use command = new SQLiteCommand(sql, connection)
        use reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg = false then 1
        else reader.["last_rowid"].ToString() |> int

    member this. getAccountNameByID(id: int): string =
        let sql = "select * from account where id = @id"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@id", id) |> ignore
        use reader = command.ExecuteReader()
        let read = reader.Read()
        if read then
            reader.["NAME"].ToString()
        else
            ""
        
        
        