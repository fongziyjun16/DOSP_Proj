// define different kinds of actors
module Actors

open System
open Akka.FSharp
open Akka.Remote
 
open Msgs

type PrinterActor() = 
    inherit Actor()

    override x.OnReceive message = 
        match box message with
        | :? PrintInfo as msg -> 
            printfn "From %s: %s" msg.SENDER msg.CONTENT
        | :? string as msg -> 
            printfn "From %s: %s" x.Sender.Path.Name msg
        | _ -> failwith "unknown message"

type StateActor() =
    inherit Actor()

    let mutable prefix = ""
    let mutable numberOfZeros = 1

    let mutable suffixLength = 1

    let mutable resultState = false

    let connector = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/connector")
    let printer = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/printer")
    let eventManager = Actor.Context.System.EventStream

    override x.OnReceive message = 
        match box message with
        | :? StartComputation as msg ->
            printer <! "Main System starts computing"
            prefix <- msg.PREFIX
            numberOfZeros <- msg.NUMBEROFZEROS
            eventManager.Publish(msg)
        | :? FindResult as msg -> 
            if resultState = false then
                resultState <- true
                printer <! "result: " + msg.RESULT + ", from " + msg.FROM
                connector <! msg
                eventManager.Publish(new StopComputation())
        | :? GetResultState as msg -> 
            x.Sender <! resultState
        | :? GetSuffixLength as msg ->
            x.Sender <! suffixLength
        | :? IncrSuffixLength as msg ->
            if msg.ORG = suffixLength then
                suffixLength <- (suffixLength + 1)
                printer <! " suffix length increases -> " + suffixLength.ToString()
                eventManager.Publish(new StopComputation())
                eventManager.Publish(new StartComputation(prefix, numberOfZeros))
        | :? SetSuffixLength as msg ->
            suffixLength <- msg.NEWVALUE
        | :? StopComputation as msg ->
            eventManager.Publish(msg)
        | _ -> failwith "unknown message"

type ConnectorActor() =
    inherit Actor()

    let mutable prefix = ""
    let mutable numberOfZeros = 1

    let stateManager = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/stateManager")
    let printer = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/printer")

    let linkedTable = new System.Collections.Generic.HashSet<string>()

    override x.OnReceive message = 
        match box message with
        | :? StartComputation as msg ->
            prefix <- msg.PREFIX
            numberOfZeros <- msg.NUMBEROFZEROS
            
            for url in linkedTable do
                let outerConnector = Actor.Context.ActorSelection(url)
                outerConnector <! x.GetOuterStartParas()
        | :? StopComputation as msg ->
            for url in linkedTable do
                let outerConnector = Actor.Context.ActorSelection(url)
                outerConnector <! new StopComputation()
        | :? ConnectionInfo as msg -> // outer system build connect
            if linkedTable.Contains(msg.FROM) = false then
                printer <! " outer system : " + msg.FROM + " connect"
                linkedTable.Add(msg.FROM) |> ignore
                if x.GetResultState() = false then
                    x.Sender <! x.GetOuterStartParas()
                else x.Sender <! new StopComputation()
        | :? OuterFindResult as msg -> // outer system find result
            stateManager <! new FindResult(msg.RESULT, "[outer: " + msg.FROM + "]")
        | :? FindResult as msg -> 
            printer <! " call outer system stop computing"
        | _ -> failwith "unknown message"

    member private x.GetOuterStartParas() =
        new OuterStartParas(prefix, numberOfZeros, x.GetSuffixLenth())

    member private x.GetSuffixLenth() = 
        int(Async.RunSynchronously((stateManager <? (new GetSuffixLength())), -1))

    member private x.GetResultState() = 
        (Async.RunSynchronously((stateManager <? (new GetResultState())), -1))

type WorkerActor() =
    inherit Actor()

    let mutable recorder = new System.Collections.Generic.List<int>()
    let mutable suffixLength = 1
    let mutable prefix = ""
    let mutable numberOfZeros = 3
    let mutable result = ""

    let printer = Actor.Context.ActorSelection("akka://" + Actor.Context.System.Name + "/user/printer")

    let mutable computationStat = true
    let mutable exceedLength = false

    override x.OnReceive message = 
        match box message with
        | :? StartComputation as msg ->
            prefix <- msg.PREFIX
            numberOfZeros <- msg.NUMBEROFZEROS

            computationStat <- true
            exceedLength <- false
            
            suffixLength <- int(Async.RunSynchronously((x.Sender <? (new GetSuffixLength())), -1))
            x.InitRecorder()

            printer <! " start computing from suffix length: " + suffixLength.ToString()

            result <- x.buildOrign()
            while computationStat && exceedLength = false &&
                        x.isValid(x.SHA256AnyString2Hex(result), numberOfZeros) = false do
                x.incrRecorder() |> ignore
                // make mainSys slowly so that simulating subSys getting data
                //if Actor.Context.System.Name.Equals("mainSys") then
                    //System.Threading.Thread.Sleep(1)
                if recorder.Count > suffixLength then
                    exceedLength <- true
                else result <- (x.buildOrign())
            
            if exceedLength = true then
                x.Sender <! new IncrSuffixLength(suffixLength, suffixLength + 1, x.Self.Path.Name)
                printer <! " exceed"
            else if computationStat && x.isValid(x.SHA256AnyString2Hex(result), numberOfZeros) then
                x.Sender <! new FindResult(result, x.Self.Path.Name)
                printer <! " get result"
            
            printer <! " stop computing"
        | :? StopComputation as msg ->
            computationStat <- false
        | _ -> failwith "unknown message"

    member private x.InitRecorder() =
        recorder <- new System.Collections.Generic.List<int>()
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



