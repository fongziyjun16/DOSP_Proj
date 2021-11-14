open System
open Microsoft.Data.Sqlite

[<EntryPoint>]
let main argv =
    
    use connection = new SqliteConnection("Data Source=exam.db")
    connection.Open()

    let testSQLCommand = "select * from student"
    let command = new SqliteCommand(testSQLCommand, connection)
    
    let reader  = command.ExecuteReader()
    while reader.Read() do
        printfn "%s:%s:%s" 
            (reader.["ufid"].ToString())
            (reader.["name"].ToString())
            (reader.["address"].ToString())

    Console.Read() |> ignore
    0 // return an integer exit code