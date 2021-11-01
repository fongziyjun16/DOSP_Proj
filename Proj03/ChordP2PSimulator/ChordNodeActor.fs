module ChordNodeActor

open System
open System.Text
open System.Numerics
open System.Globalization
open System.Collections.Generic

open Akka.Actor
open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open ToolsKit
open Msgs

type ChordNodeActor(identifier: string, numberOfRequests: int) =
    inherit Actor()

    let context = Actor.Context
    let mutable predecessor = ""
    let mutable successor = identifier
    let basePosition = ToolsKit.toBigInteger(identifier)

    let offsetAddrs = new List<string>()
    let fingerTable = new List<string>()

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let generateOneRandomResource(): string =
        let random = new Random()
        let letters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        let randomLength = random.Next(10, 41)
        let randomResource = new StringBuilder()
        for i in 1 .. randomLength do
            randomResource.Append(letters.[random.Next(0, letters.Length)]) |> ignore
        randomResource.ToString()

    let findSuccessor(id: string): string = 
        let idCode = BigInteger.Parse(id, NumberStyles.HexNumber)
        let currCode = ToolsKit.toBigInteger(identifier)
        let successorCode = ToolsKit.toBigInteger(successor)

        let mutable next = ""
        if currCode.CompareTo(successorCode) = 0 then next <- successor
        else if currCode.CompareTo(successorCode) < 0 then
            if idCode.CompareTo(currCode) > 0 && idCode.CompareTo(successorCode) <= 0 then
                next <- successor
        else
            if idCode.CompareTo(currCode) > 0 && idCode.CompareTo(ToolsKit.getMAX()) <= 0 || 
                idCode.CompareTo(ToolsKit.getMIN()) >= 0 && idCode.CompareTo(successorCode) <= 0 then
                next <- successor

        if next = successor then successor |> ignore
        else
            let mutable index = 159
            let mutable found = false
            while found <> true && index >= 0 do
                let nextNode = fingerTable.[index]
                let nextNodeCode = ToolsKit.toBigInteger(nextNode) 
                if currCode.CompareTo(idCode) < 0 then
                    if nextNodeCode.CompareTo(currCode) > 0 && nextNodeCode.CompareTo(idCode) < 0 then
                        next <- nextNode
                        found <- true
                else
                    if nextNodeCode.CompareTo(currCode) > 0 && nextNodeCode.CompareTo(ToolsKit.getMAX()) <= 0 || 
                        nextNodeCode.CompareTo(ToolsKit.getMIN()) >= 0 && nextNodeCode.CompareTo(idCode) < 0 then
                        next <- nextNode
                        found <- true
                index <- index - 1
        next

    let updSuccessor(newSuccessor: string) =
        successor <- newSuccessor
        for i in 0 .. 159 do
            fingerTable.[i] <- successor

    let updPredecessor(newPredecessor: string) =
        predecessor <- newPredecessor
        updSuccessor(successor)

    let notify(other: string) =
        let otherCode = ToolsKit.toBigInteger(other)
        let currCode = ToolsKit.toBigInteger(identifier)
        let predecessorCode = ToolsKit.toBigInteger(predecessor)
        if predecessor = "" || currCode.CompareTo(predecessorCode) = 0 then
            updPredecessor(other)
        else
            if predecessorCode.CompareTo(currCode) < 0 then
                if otherCode.CompareTo(predecessorCode) > 0 && otherCode.CompareTo(currCode) < 0 then
                    updPredecessor(other)
            else 
                if otherCode.CompareTo(predecessorCode) > 0 && otherCode.CompareTo(ToolsKit.getMAX()) <= 0 ||
                    otherCode.CompareTo(ToolsKit.getMIN()) >= 0 && otherCode.CompareTo(currCode) < 0 then
                    updPredecessor(other)

    let mutable msgSeqNumber = 0
    let getNextMsgSeqNumber(): int = 
        msgSeqNumber <- msgSeqNumber + 1
        msgSeqNumber

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

        predecessor <- "" 
        successor <- identifier
        // initialize finger table
        for i in 0 .. 159 do
            fingerTable.Add(identifier) |> ignore

            let powerOf2 = i |> fun x ->
                                    let test = new StringBuilder("1" + new String('0', x))
                                    if test.Length % 4 <> 0 then
                                        let counter = test.Length % 4
                                        test.Insert(0, new String('0', 4 - counter)) |> ignore
                                    let hexString = test |> fun test ->
                                                                let mutable index = 0
                                                                let hexString = new StringBuilder()
                                                                while index < test.Length do
                                                                    hexString.Append(Convert.ToInt32(test.ToString(index, 4), 2).ToString("X")) |> ignore
                                                                    index <- index + 4
                                                                hexString.ToString()
                                    "0" + hexString
            let offset = BigInteger.Parse(powerOf2, NumberStyles.HexNumber)
            let nextPosition = BigInteger.Add(basePosition, offset) % ToolsKit.getMAX()
            offsetAddrs.Add("0" + nextPosition.ToString("X"))
        // let basePositionHex = basePosition.ToString("X")
        // printfn "created"
            
    override this.OnReceive message =
        match box message with
        | :? AskPredecessor as msg ->
            this.Sender <! predecessor
        | :? FindSuccessor as msg ->
            let next = findSuccessor(ToolsKit.encodeBySHA1(msg.ID))
            if next = identifier || next = successor then 
                this.Sender <! next
            else
                let target = Actor.Context.System.ActorSelection(Actor.Context.Self.Path.Root.ToString() + "/user/" + next)
                this.Sender <! Async.RunSynchronously(target <? new FindSuccessor(msg.ID), -1)
        | :? Join as msg ->
            updPredecessor("")
            let target = Actor.Context.System.ActorSelection(Actor.Context.Self.Path.Root.ToString() + "/user/" + msg.ID)
            updSuccessor((string) (Async.RunSynchronously(target <? new FindSuccessor(identifier), -1)))
        | :? Stabilize as msg ->
            let context = Actor.Context
            async {
                while true && ToolsKit.getPeriodSwitch() do
                    let mutable successorPredecessor = context |> fun context ->
                                                                    if identifier.Equals(successor) then ""
                                                                    else 
                                                                        let target = context.System.ActorSelection(context.Self.Path.Root.ToString() + "/user/" + successor)
                                                                        (string) (Async.RunSynchronously(target <? new AskPredecessor(), -1))
                    if successorPredecessor <> "" then
                        let successorPredecessorCode = ToolsKit.toBigInteger(successorPredecessor)
                        let currCode = ToolsKit.toBigInteger(identifier)
                        let successorCode = ToolsKit.toBigInteger(successor)

                        if currCode.CompareTo(successorCode) < 0 then
                            if successorPredecessorCode.CompareTo(currCode) > 0 && successorPredecessorCode.CompareTo(successorCode) < 0 then
                                updSuccessor(successorPredecessor)
                        else 
                            if successorPredecessorCode.CompareTo(currCode) > 0 && successorPredecessorCode.CompareTo(ToolsKit.getMAX()) <= 0 ||
                                successorPredecessorCode.CompareTo(ToolsKit.getMIN()) >= 0 && successorPredecessorCode.CompareTo(currCode) < 0 then
                                updSuccessor(successorPredecessor)
                        
                    mediator <! new Send("/user/" + successor, new Notify(identifier), true)
                    do! Async.Sleep(500)
                mediator <! new Send("/user/printer", identifier + " stop stabilize period", true)
            } |> Async.StartAsTask |> ignore
        | :? Notify as msg ->
            notify(msg.ID)
        | :? UpdSuccessor as msg ->
            updSuccessor(msg.ID)
        | :? FixFingerTable as msg ->
            async {
                while true && ToolsKit.getPeriodSwitch() do
                    let mutable next = 0
                    while next <> 160 do
                        let nextNode = findSuccessor(offsetAddrs.[next])
                        fingerTable.[next] <- nextNode
                        next <- next + 1
                    do! Async.Sleep(500)
                mediator <! new Send("/user/printer", identifier + " stop fix finger table period", true)
            } |> Async.StartAsTask |> ignore
        | :? PrintContextInfo as msg ->
            mediator <! new Send("/user/printer", "predecessor: " + predecessor + "; successor: " + successor, true)
        | :? AskNodeContext as msg ->
            this.Sender <! predecessor + ":" + successor
        // Start request
        | :? StartMission as msg ->
            let mission = Actor.Context.System
                            .Scheduler.ScheduleTellRepeatedlyCancelable(
                                    TimeSpan.FromSeconds(0.0),
                                    TimeSpan.FromSeconds(1.0),
                                    Actor.Context.Self,
                                    new PreLookup(),
                                    ActorRefs.NoSender
                                ) 
            mission.CancelAfter(numberOfRequests * 1000)
        | :? PreLookup as msg ->
            let lookupInfo = new Lookup(generateOneRandomResource(), identifier, getNextMsgSeqNumber())
            mediator <! new Send("/user/printer", identifier + " looks up [" + lookupInfo.getSeqNumber().ToString() +  "] " + lookupInfo.getKey(), true)
            mediator <! new Send("/user/" + identifier, lookupInfo, true)
        | :? Lookup as msg ->
            let next = findSuccessor(ToolsKit.encodeBySHA1(msg.getKey()))
            if next = successor then 
                let foundMsg = new FoundResource(msg.getSteps(), msg.getPublisher(), msg.getSeqNumber())
                mediator <! new Send("/user/chordManager", foundMsg, true)
            else
                msg.incrSteps()
                mediator <! new Send("/user/" + next, msg, true)
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name


        
