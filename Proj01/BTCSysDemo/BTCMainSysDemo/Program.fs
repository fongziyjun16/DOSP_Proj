// BTC Main Sys Demo
// one printer actor, manage printing
// one state actor, manage computing state
// one connector actor, manage connection and communication with outer system connector
// more than one computing workers, computing
open Akka.Actor
open Akka.FSharp
open Akka.Remote
open Akka.Configuration

open Msgs
open Actors

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

let sysName = "mainSys"
let localAddrBase = "akka://" + sysName + "/user"
let mainSys = System.create sysName (configuration)
let eventManager = mainSys.EventStream


[<EntryPoint>]
let main argv =
    let startInfo = new StartComputation("yingjie.chen", 5)
    
    let printer = mainSys.ActorOf(Props(typedefof<PrinterActor>), "printer")
    printer <! new PrintInfo(
                    sysName, 
                    "computation paras : " + startInfo.PREFIX + ":" + startInfo.NUMBEROFZEROS.ToString())

    let stateManager = mainSys.ActorOf(Props(typedefof<StateActor>), "stateManager")

    let connector = mainSys.ActorOf(Props(typedefof<ConnectorActor>), "connector")
    eventManager.Subscribe(connector, typedefof<StartComputation>) |> ignore
    eventManager.Subscribe(connector, typedefof<StopComputation>) |> ignore

    let localWorkers = 
        [1 .. 5]
        |> List.map(fun id -> 
            let worker = mainSys.ActorOf(Props(typedefof<WorkerActor>), "local_worker" + id.ToString())
            eventManager.Subscribe(worker, typedefof<StartComputation>) |> ignore
            eventManager.Subscribe(worker, typedefof<StopComputation>) |> ignore
            printer <! new PrintInfo(
                            sysName, 
                            "generate " + worker.Path.Name)
            worker)
    
    stateManager <! startInfo

    System.Console.Read() |> ignore
    0