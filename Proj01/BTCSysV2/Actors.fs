// define various kinds of actors

module Actors

open System
open Akka.FSharp

open Msgs

type PrinterActor(isMainSys: bool) =
    inherit Actor()

    let connector = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/connector")

    override x.OnReceive message =
        if isMainSys then
            match box message with
            | :? OneTuple as msg ->
                if msg.LEADINGZEROS > 0 then
                    printfn "[%s]\twith [%d] leading zeros\t[0x%s]" msg.COIN msg.LEADINGZEROS msg.SHA256
            | :? PrintingInfo as msg ->
                printfn "From [%s] 's message: [%s]" msg.FROM msg.CONTENT
            | :? string as msg ->
                printfn "From [%s] 's message: [%s]" x.Sender.Path.Name msg
            | _ -> printf "unknown message"
        else
            match box message with
            | :? PrintingInfo as msg ->
                connector <! new OuterPrintingInfo(msg.FROM, msg.CONTENT)
            | :? string as msg ->
                connector <! new OuterPrintingInfo(x.Sender.Path.Name, msg)
            | _ -> printf "unknown message"

type StateManagementActor(isMainSys: bool, prefix: string, 
                            numberOfZeros: int, numberOfWorkers: int) =
    inherit Actor()

    let connector = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/connector")

    let printer = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/printer")

    let mutable suffixLength = 1
    let mutable sameComputingNumber = 0

    let resultSet = System.Collections.Generic.HashSet<string>()
    let suffixResultLengthSet = System.Collections.Generic.HashSet<int>()

    let eventManager = Actor.Context.System.EventStream

    override x.OnReceive message =
        if isMainSys then // main system
            match box message with
            | :? StartComputing as msg ->
                eventManager.Publish(msg)
            | :? FoundOneResult as msg ->
                if resultSet.Contains(msg.RESULT) = false then
                    resultSet.Add(msg.RESULT) |> ignore
                    printer <! new PrintingInfo(msg.FROM, msg.RESULT)
            | :? OneTuple as msg ->
                if msg.LEADINGZEROS = numberOfZeros then
                    if resultSet.Contains(msg.COIN) = false then
                        resultSet.Add(msg.COIN) |> ignore
                        printer <! msg
                else
                    if suffixResultLengthSet.Contains(msg.LEADINGZEROS) = false then
                        suffixResultLengthSet.Add(msg.LEADINGZEROS) |> ignore
                        printer <! msg
            | :? GetSuffixLength as msg ->
                if numberOfWorkers < 3 then
                    x.Sender <! suffixLength
                    suffixLength <- (suffixLength + 1)
                else
                    if sameComputingNumber < 3 then
                        sameComputingNumber <- (sameComputingNumber + 1)
                    else 
                        sameComputingNumber <- 1
                        suffixLength <- (suffixLength + 1)
                    x.Sender <! suffixLength
            | :? OuterStartComputing as msg ->
                x.Sender <! new OuterStartComputing(prefix, numberOfZeros)
            | _ -> printer <! "unknown message"
        else // sub system
            match box message with
            | :? StartComputing as msg ->
                connector <! new OuterStartComputing()
            | :? OuterStartComputing as msg ->
                eventManager.Publish(new StartComputing(msg.PREFIX, msg.NUMBEROFZEROS))
            | :? FoundOneResult as msg ->
                connector <! msg
            | :? OneTuple as msg ->
                connector <! msg
            | :? GetSuffixLength as msg ->
                x.Sender <! Async.RunSynchronously((connector <? msg), -1)
            | _ -> printer <! "unknown message"
            
type ConnectionActor(isMainSys: bool, mainSysAddrBase: string) =
    inherit Actor()

    let printer = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/printer")

    let mainSysConnector = Actor.Context.ActorSelection(mainSysAddrBase + "/connector")

    let stateManager = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/stateManager")

    override x.OnReceive message =
        if isMainSys then // main system
            match box message with
            | :? OuterPrintingInfo as msg ->
                printer <! new PrintingInfo(msg.FROM, msg.CONTENT)
            | :? FoundOneResult as msg ->
                stateManager <! msg
            | :? OneTuple as msg ->
                stateManager <! msg
            | :? GetSuffixLength as msg ->
                x.Sender <! Async.RunSynchronously((stateManager <? msg), -1)
            | :? OuterStartComputing as msg ->
                x.Sender <! Async.RunSynchronously((stateManager <? msg), -1)
                printer <! new PrintingInfo(x.Sender.Path.ToStringWithAddress(), " join computation")
            | _ -> printer <! "unknown message"
        else // sub system
            match box message with
            | :? OuterStartComputing as msg ->
                x.Sender <! Async.RunSynchronously((mainSysConnector <? msg), -1)
            | :? OuterPrintingInfo as msg ->
                mainSysConnector <! msg
            | :? FoundOneResult as msg ->
                mainSysConnector <! msg
            | :? OneTuple as msg ->
                mainSysConnector <! msg
            | :? GetSuffixLength as msg ->
                x.Sender <! Async.RunSynchronously((mainSysConnector <? msg), -1)
            | _ -> printer <! "unknown message"

type WorkActor() =
    inherit Actor()

    let stateManager = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/stateManager")

    let printer = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/printer")

    let mutable recorder = new Collections.Generic.List<int>()
    let mutable suffixLength = -1
    let mutable prefix = ""
    let mutable numberOfZeros = 3
    let mutable result = ""
    let lengthSet = Collections.Generic.HashSet<int>()

    override x.OnReceive message =
        match box message with
        | :? StartComputing as msg ->
            prefix <- msg.PREFIX
            numberOfZeros <- msg.NUMBEROFZEROS

            while true do
                suffixLength <- int(Async.RunSynchronously((stateManager <? new GetSuffixLength(suffixLength)), -1))
                if lengthSet.Contains(suffixLength) = false then
                    lengthSet.Add(suffixLength) |> ignore
(*                    printer <! new PrintingInfo(
                                    x.Self.Path.ToStringWithAddress(), 
                                    " start computing with suffixlength " + suffixLength.ToString())
*)                
                    x.InitRecorder()
                
                    while recorder.Count <= suffixLength do
                        result <- (x.buildOrign())
                        // if Actor.Context.System.Name.Equals("mainSys") then
                            // System.Threading.Thread.Sleep(100)
                        let sha256: string = x.SHA256AnyString2Hex(result)
                        if '0' = sha256.[0] then
                            stateManager <! new OneTuple(result, sha256, x.CountLeadingZeros(sha256))
(*                        if x.isValid(sha256, numberOfZeros) = true then
                            // stateManager <! new FoundOneResult(result, x.Self.Path.ToStringWithAddress())
                            stateManager <! new OneTuple(result, sha256)
*)
                        x.incrRecorder()
                    
        | _ -> printer <! "unknown message"

    member private x.InitRecorder() =
        recorder <- new Collections.Generic.List<int>()
        for i in 1 .. suffixLength do
            recorder.Add(32)

    member private x.incrRecorder() =
        let mutable extra = 0;
        recorder.[recorder.Count - 1] <- recorder.[recorder.Count - 1] + 1
        if recorder.[recorder.Count - 1] = 127 then
            extra <- 1
            recorder.[recorder.Count - 1] <- 32
    
        let mutable counter = recorder.Count - 2
        while extra <> 0 && counter >= 0 do
            recorder.[counter] <- recorder.[counter] + extra
            extra <- 0
            if recorder.[counter] = 127 then
                extra <- 1
                recorder.[counter] <- 32
            counter <- counter - 1

        if extra = 1 then
            recorder.Insert(0, 32)

    member x.CountLeadingZeros (sha256: string): int =
        let mutable count = 0;
        let mutable i = 0;
        while i < sha256.Length && sha256.[i] = '0' do
            count <- (count + 1)
            i <- (i + 1)
        count

    member private x.SHA256AnyString2Hex (str : string) =
        Security.Cryptography.SHA256.Create().ComputeHash(Text.Encoding.Default.GetBytes str)
        |> Array.map (fun (x : byte) -> String.Format("{0:X2}", x))
        |> String.concat String.Empty

    member private x.isValid (str : string, zeros : int) =
        let zerosSB = new Text.StringBuilder();
        for i in 1 .. zeros do
            zerosSB.Append('0') |> ignore
        let pattern = Text.RegularExpressions.Regex("^" + zerosSB.ToString() + ".*$");
        pattern.IsMatch(str)

    member private x.buildOrign () =
        let orgPrefix = new Text.StringBuilder(prefix)
        for charIntValue in recorder do
            orgPrefix.Append(Convert.ToChar charIntValue) |> ignore
        orgPrefix.ToString()

