open System
open System.Diagnostics

printfn "process ID: %d" (Process.GetCurrentProcess().Id)

let loopTest x = 
    async {
        while true do
            let i = 1
            0
    }

let count = 2;
for i in 1 .. count do
    loopTest 123
    |> Async.Start

Console.Read() |> ignore