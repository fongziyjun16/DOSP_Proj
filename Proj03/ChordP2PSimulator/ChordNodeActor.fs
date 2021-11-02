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

type ChordNodeChildActor() =
    inherit Actor()

    let mutable predecessor = ""

    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message =
        match box message with
        | :? UpdPredecessor as msg ->
            predecessor <- msg.ID
        | :? AskPredecessor as msg ->
            this.Sender <! predecessor
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name

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

    let isContained(id: string): bool =
        let idCode = BigInteger.Parse("0" + id, NumberStyles.HexNumber)
        let currCode = ToolsKit.toBigInteger(identifier)
        let successorCode = ToolsKit.toBigInteger(successor)

        let mutable flg = false
        if currCode.CompareTo(successorCode) = 0 then flg <- true
        else if currCode.CompareTo(successorCode) < 0 then
            if idCode.CompareTo(currCode) > 0 && idCode.CompareTo(successorCode) <= 0 then
                flg <- true
        else
            if idCode.CompareTo(currCode) > 0 && idCode.CompareTo(ToolsKit.getMAX()) <= 0 || 
                idCode.CompareTo(ToolsKit.getMIN()) >= 0 && idCode.CompareTo(successorCode) <= 0 then
                flg <- true

        flg

    let findInFingerTable(id: string): string =
        let idCode = BigInteger.Parse("0" + id, NumberStyles.HexNumber)
        let currCode = ToolsKit.toBigInteger(identifier)

        let mutable next = identifier
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

    let findSuccessor(id: string): string = 

        let mutable next = isContained(id) |> fun x -> if x then successor else identifier

        if next <> successor then 
            next <- findInFingerTable(id)

        next

    let continueFindSuccessor(id: string, encoded: bool): string =
        let idHexCode = id |> fun x -> if encoded then id else ToolsKit.encodeBySHA1(id)
        if isContained(idHexCode) then successor
        else
            let next = findInFingerTable(idHexCode)
            if next = identifier then identifier
            else
                let target = context.System.ActorSelection(context.Self.Path.Root.ToString() + "/user/" + next)
                (string) (Async.RunSynchronously(target <? new FindSuccessor(id, encoded), -1))
            
    let updSuccessor(newSuccessor: string) =
        successor <- newSuccessor
        for i in 0 .. 159 do
            fingerTable.[i] <- successor

    let updPredecessor(newPredecessor: string) =
        predecessor <- newPredecessor
        mediator <! new Send("/user/" + identifier + "/child", new UpdPredecessor(predecessor), true)
        updSuccessor(successor)

    let mutable msgSeqNumber = 0
    let getNextMsgSeqNumber(): int = 
        msgSeqNumber <- msgSeqNumber + 1
        msgSeqNumber

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

        context.ActorOf(Props(typeof<ChordNodeChildActor>), "child") |> ignore

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
            
    override this.OnReceive message =
        match box message with
        | :? FindSuccessor as msg ->
            this.Sender <! continueFindSuccessor(msg.ID, msg.ENCODED)
        | :? Join as msg ->
            predecessor <- ""
            let target = Actor.Context.System.ActorSelection(Actor.Context.Self.Path.Root.ToString() + "/user/" + msg.ID)
            updSuccessor((string) (Async.RunSynchronously(target <? new FindSuccessor(identifier, false), -1)))
            // successor <- (string) (Async.RunSynchronously(target <? new FindSuccessor(identifier, false), -1))
        | :? Stabilize as msg ->
            let mutable successorPredecessor = ""
            if identifier.Equals(successor) = false then
                let target = context.System.ActorSelection(context.Self.Path.Root.ToString() + "/user/" + successor + "/child")
                successorPredecessor <- (string) (Async.RunSynchronously(target <? new AskPredecessor(), -1))
            
            if successorPredecessor.Equals("") = false then
                let successorPredecessorCode = ToolsKit.toBigInteger(successorPredecessor)
                let currCode = ToolsKit.toBigInteger(identifier)
                let successorCode = ToolsKit.toBigInteger(successor)

                if currCode.CompareTo(successorCode) < 0 then
                    if successorPredecessorCode.CompareTo(currCode) > 0 && successorPredecessorCode.CompareTo(successorCode) < 0 then
                        // successor <- successorPredecessor
                        updSuccessor(successorPredecessor)
                else
                    if successorPredecessorCode.CompareTo(currCode) > 0 && successorPredecessorCode.CompareTo(ToolsKit.getMAX()) <= 0 
                        || successorPredecessorCode.CompareTo(ToolsKit.getMIN()) >= 0 && successorPredecessorCode.CompareTo(successorCode) < 0 then
                        // successor <- successorPredecessor
                        updSuccessor(successorPredecessor)
                
            mediator <! new Send("/user/" + successor, new Notify(identifier), true)
        | :? Notify as msg ->
            let other = msg.ID
            let otherCode = ToolsKit.toBigInteger(other)
            let currCode = ToolsKit.toBigInteger(identifier)
            let predecessorCode = ToolsKit.toBigInteger(predecessor)
            if predecessor.Equals("") then
                updPredecessor(other)
                //predecessor <- other
            else
                if predecessorCode.CompareTo(currCode) < 0 then
                    if otherCode.CompareTo(predecessorCode) > 0 && otherCode.CompareTo(currCode) < 0 then
                        updPredecessor(other)
                        // predecessor <- other
                else 
                    if otherCode.CompareTo(predecessorCode) > 0 && otherCode.CompareTo(ToolsKit.getMAX()) <= 0 ||
                        otherCode.CompareTo(ToolsKit.getMIN()) >= 0 && otherCode.CompareTo(currCode) < 0 then
                            updPredecessor(other)
                            // predecessor <- other
        | :? UpdSuccessor as msg ->
            updSuccessor(msg.ID)
        | :? FixFingerTable as msg ->
            let mutable next = 0
            while next <> 160 do
                let key = offsetAddrs.[next]
                let mutable nextNode = continueFindSuccessor(key, true)
                fingerTable.[next] <- nextNode
                next <- next + 1
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


        
