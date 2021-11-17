namespace DAO

open System.Data.SQLite

open Entities

type AccountDAO(connection: SQLiteConnection) =

    member this.insert(account: Account): bool =
        let sql = "insert into account values(@name)"
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
        let reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg then
            new Account(
                reader.["NAME"].ToString()
            )
        else
            new Account("")


