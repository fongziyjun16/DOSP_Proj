open System
open System.Data.SQLite

open Entities
open DAO

[<EntryPoint>]
let main argv =
    
    use connection = new SQLiteConnection("Data Source=./resources/tweet_sys.db")
    connection.Open()

    connection.Close()
    Console.Read() |> ignore
    0 // return an integer exit code