// simulate outer system
open Akka.Actor
open Akka.FSharp
open Akka.Remote
open Akka.Configuration

open Msgs
open Actors
open SubActors

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

let sysName = "subSys"
let localAddrBase = "akka://" + sysName + "/user"
let mainSys = System.create sysName (configuration)
let eventManager = mainSys.EventStream

[<EntryPoint>]
let main argv =
    // let startInfo = new StartComputation("yingjie.chen", 5)
    let mainSysConnectorInfo = new MainSysConnectorInfo(
                                    "mainSys", "localhost", 9090, "connector")

    let printer = mainSys.ActorOf(Props(typedefof<PrinterActor>), "printer")
(*    printer <! new PrintInfo(
                    sysName, 
                    "computation paras : " + startInfo.PREFIX + ":" + startInfo.NUMBEROFZEROS.ToString())
*)
    
    let stateManager = mainSys.ActorOf(Props(typedefof<StateActor>), "stateManager")
    
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

    let connector = mainSys.ActorOf(Props(typedefof<SubConnector>), "connector")
    // eventManager.Subscribe(connector, typedefof<StartComputation>) |> ignore
    // eventManager.Subscribe(connector, typedefof<StopComputation>) |> ignore

    let subSysConnectorInfo = new ConnectionInfo(
                                        "akka.tcp://" + sysName + "@localhost:9091/user/connector", 
                                        true)

    connector <! mainSysConnectorInfo
    connector <! subSysConnectorInfo // build connection with main sys and get mission

    System.Console.Read() |> ignore
    0