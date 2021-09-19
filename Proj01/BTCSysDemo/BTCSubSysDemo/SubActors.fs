// define specific actors for SubSystem
module SubActors

open System
open Akka.FSharp
open Akka.Remote

open Msgs

type SubConnector() =
    inherit Actor()

    let mutable prefix = ""
    let mutable numberOfZeros = 1

    let mutable mainSysConnectorInfo = new MainSysConnectorInfo()
    let mutable mainSysConnector = null

    let stateManager = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/stateManager")
    let printer = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/printer")

    override x.OnReceive message =
        match box message with
        | :? ConnectionInfo as msg ->
            printer <! " start connecting to main system"
            mainSysConnector <! msg
        | :? OuterStartParas as msg ->
            prefix <- msg.PREFIX
            numberOfZeros <- msg.NUMBEROFZEROS
            let startInfo = new StartComputation(msg.PREFIX, msg.NUMBEROFZEROS)
            stateManager <! new SetSuffixLength(msg.SUFFIXLENGTH)
            printer <! " get prefix: " + prefix + " ; numberOfZeros: " + numberOfZeros.ToString() + " ; suffix length: " + msg.SUFFIXLENGTH.ToString()
            stateManager <! startInfo
        | :? StopComputation as msg ->
            stateManager <! msg
        | :? MainSysConnectorInfo as msg ->
            printer <! " set main system info"
            x.SetMainSysConnectorInfo(msg)
            let mainSysConnectorUrl = "akka.tcp://" + 
                                        mainSysConnectorInfo.MAINSYSNAME + "@" +
                                        mainSysConnectorInfo.IP + ":" +
                                        mainSysConnectorInfo.PORT.ToString() + "/user/" +
                                        mainSysConnectorInfo.CONNECTORNAME
            mainSysConnector <- Actor.Context.ActorSelection(mainSysConnectorUrl)
        | :? FindResult as msg ->
            mainSysConnector <! new OuterFindResult(msg.RESULT, msg.FROM)
            stateManager <! new StopComputation()
        | _ -> failwith "unknown message"
    
    member private x.SetMainSysConnectorInfo(info: MainSysConnectorInfo) =
        mainSysConnectorInfo <- info

