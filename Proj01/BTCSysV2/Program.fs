// BTCSysV2

open Akka.Actor
open Akka.FSharp
open Akka.Remote
open Akka.Configuration

open Msgs
open Actors

[<EntryPoint>]
let main argv =
    if argv.Length = 4 then
        // parameters definition
        let ipv4 = argv.[0]
        let port = argv.[1]
        let isMainSys = argv.[2] |> fun sign -> 
                                        if sign.Equals("y") then true
                                        else false
        let numberOfWorkers = argv.[3] |> int

        let mutable prefix = ""
        if isMainSys then
            printf "please enter the prefix: "
            prefix <- System.Console.ReadLine()

        let mutable numberOfZeros = 1
        if isMainSys then
            printf "please enter the number of leading zeros: "
            numberOfZeros <- (System.Console.ReadLine() |> int)

        let mutable mainSysIPv4 = ""
        if isMainSys = false then
            printf "please enter the main system IPv4 Address: "
            mainSysIPv4 <- System.Console.ReadLine()

        let mutable mainSysPort = ""
        if isMainSys = false then
            printf "please enter the main system Port:"
            mainSysPort <- System.Console.ReadLine()

        let mainSysAddrBase = isMainSys |> fun main ->
                                            if main then
                                                ""
                                            else
                                                "akka.tcp://mainSys@" + mainSysIPv4 + ":" + mainSysPort + "/user"

        // system definition

        let sysName = isMainSys |> fun main ->
                                    if main then "mainSys"
                                    else "subSys"

        printfn "this system will start......"
        printfn "================ SYS INFO ================"

        let configuration = ConfigurationFactory.ParseString(@"
                                akka {
                                    actor.provider = remote
                                    remote {
                                        dot-netty.tcp {
                                            port = " + port + "
                                            hostname = " + ipv4 + "
                                        }
                                    }
                                }")

        let sys = System.create sysName (configuration)

        let printer = sys.ActorOf(Props(typedefof<PrinterActor>, [| isMainSys :> obj |]), "printer")

        let stateManager = sys.ActorOf(Props(typedefof<StateManagementActor>, [| isMainSys :> obj; prefix :> obj; numberOfZeros :> obj; numberOfWorkers :> obj |]), "stateManager")

        let connector = sys.ActorOf(Props(typedefof<ConnectionActor>, [| isMainSys :> obj; mainSysAddrBase :> obj |]), "connector")

        let eventManager = sys.EventStream

        let workers =
            [1 .. numberOfWorkers]
            |> List.map (fun id ->
                            let worker = sys.ActorOf(Props(typedefof<WorkActor>), "worker_" + id.ToString())
                            eventManager.Subscribe(worker, typedefof<StartComputing>) |> ignore
                            // printer <! new PrintingInfo(sysName, " generate " + worker.Path.Name)
                            worker)

        // start computing

        if isMainSys then // main system mode
            stateManager <! new StartComputing(prefix, numberOfZeros)
        else // sub system mode
            stateManager <! new StartComputing()

    else
        printfn "please run with parameters: local IPv4 Address, Port, y/n (y -- main system, n -- Sub System), Number (the number of workers)"
    
    System.Console.Read() |> ignore
    0

