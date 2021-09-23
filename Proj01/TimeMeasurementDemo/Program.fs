// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Diagnostics

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom

[<EntryPoint>]
let main argv =
    let timer = new Stopwatch()
    let message = from "F#" // Call the function
    printfn "Hello world %s" message
    printfn "Elapsed Time: %i" timer.ElapsedMilliseconds
    let messageB = from "C#" // Call the function
    printfn "Hello world %s" messageB
    printfn "Elapsed Time: %i" timer.ElapsedMilliseconds
    0 // return an integer exit code