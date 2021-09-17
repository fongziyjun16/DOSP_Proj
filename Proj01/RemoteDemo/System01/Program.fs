// System01 simulates a responsor
open Akka.FSharp
open Akka.Remote
open Akka.Configuration

let configuration = ConfigurationFactory.ParseString(@"
        akka {
            actor.provider = remote
            remote {
                dot-netty.tcp {
                    port = 9090
                    hostname = localhost
                }
            }
        }
    ")

// Akka System
let sys01 = System.create "sys01" (configuration)

// actor
let smith = 
    spawn sys01 "smith"
        (fun mailbox ->
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? string ->
                    printfn "smith receives message: %s" message
                    sender <! "Hello, I am Smith. Getting your message successfully"
                    return! loop()
                | _ -> failwith "unknown error"
            } 
            loop())

[<EntryPoint>]
let main argv =
    // simulating a responsor, just waiting
    
    System.Console.Read() |> ignore
    0




