open System
open Akka.Actor
open Akka.FSharp
open Akka.Configuration

open Msgs
open Actors

let systemName = "PSFNSystem"

[<EntryPoint>]
let main argv =

    let configuration = ConfigurationFactory.ParseString(@"
                            akka {
                                actor.provider = cluster
                                remote {
                                    dot-netty.tcp {
                                        port = 9090
                                        hostname = localhost
                                    }
                                }
                                cluster {
                                    seed-nodes = [""akka.tcp://PSFNSystem@localhost:9090""]
                                }
                            }
                        ")

    let PSFNSystem = System.create systemName (configuration)



    Console.Read() |> ignore
    0 // return an integer exit code