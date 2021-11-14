namespace DAO

open System.Data.SQLite

open Entities

type AccountDAO(connection: SQLiteConnection) =

    member this.insert(account: Account): bool =
        let sql = "insert into account values(@username, @password, @name, @gender)"
        use command = new SQLiteCommand(sql, connection)
        command.Parameters.AddWithValue("@username", account.USERNAME) |> ignore
        command.Parameters.AddWithValue("@password", account.PASSWORD) |> ignore
        command.Parameters.AddWithValue("@name", account.NAME) |> ignore
        command.Parameters.AddWithValue("@gender", account.GENDER) |> ignore
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
        let reader = command.ExecuteReader()
        let flg = reader.Read()
        if flg then
            new Account(
                reader.["USERNAME"].ToString(),
                reader.["PASSWORD"].ToString(),
                reader.["NAME"].ToString(),
                reader.["GENDER"].ToString()
            )
        else
            new Account("", "", "", "")


