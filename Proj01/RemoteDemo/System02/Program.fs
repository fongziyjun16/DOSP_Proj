// System02 simulates a requester
open Akka.FSharp
open Akka.Remote
open Akka.Configuration

let configuration = ConfigurationFactory.ParseString(@"
        akka {
            actor.provider = remote
            remote {
                dot-netty.tcp {
                    port = 9091
                    hostname = localhost
                }
            }
        }
    ")

// Akka System
let sys02 = System.create "sys02" (configuration)

// actor
let smith = sys02.ActorSelection("akka.tcp://sys01@localhost:9090/user/smith")

[<EntryPoint>]
let main argv =
    System.Threading.Thread.Sleep(1000) // wait for System01 starting

    let task = smith <? "Message from Jedi"

    let response = Async.RunSynchronously(task, 3000)

    printfn "response info: %s" (string(response))

    System.Console.Read() |> ignore
    0 // return an integer exit code