module ChordNodeInfo

open System.Numerics
open System.Globalization
open System.Collections.Generic

open ToolsKit

type ChordNodeInfo(identifier: string) =
    
    let mutable predecessor = ""
    let mutable successor = ""
    
    let mutable identifierCode = new BigInteger(0)
    let mutable predecessorCode = new BigInteger(0)
    let mutable successorCode = new BigInteger(0)

    let offsetAddrs = new List<string>()
    let fingerTable = new List<string>()

    let mutable stabilizeFlg = true
    let mutable fixFingerTableFlg = true

    let resetFingerTable() =
        if fingerTable.Count = 160 then
            for i in 0 .. 159 do
                fingerTable.[i] <- successor

    member this.initialize() =
        successor <- identifier

        identifierCode <- ToolsKit.toBigInteger(identifier)
        this.setPredecessor(predecessor)
        this.setSuccessor(successor)

        for i in 0 .. 159 do
            fingerTable.Add(identifier) |> ignore
            let powerOf2 = ToolsKit.calculatePowerOf2(i)
            let offset = BigInteger.Parse(powerOf2, NumberStyles.HexNumber)
            let nextPosition = BigInteger.Add(ToolsKit.toBigInteger(identifier), offset) % ToolsKit.getMAX()
            offsetAddrs.Add("0" + nextPosition.ToString("X"))

    member this.isInSuccessor(key: string): bool =
        let keyCode = BigInteger.Parse("0" + key, NumberStyles.HexNumber)
        if identifierCode.CompareTo(successorCode) = 0 then true
        else ToolsKit.isInScope(keyCode, identifierCode, successorCode, true, false)

    member this.findInFigerTable(key: string): string =
        let keyCode = BigInteger.Parse("0" + key, NumberStyles.HexNumber)
        let mutable next = identifier
        let mutable index = 159
        let mutable found = false
        while found = false && index >= 0 do
            let nextNode = fingerTable.[index]
            let nextNodeCode = ToolsKit.toBigInteger(nextNode) 
            if ToolsKit.isInScope(nextNodeCode, identifierCode, keyCode, true, true) then
                next <- nextNode
                found <- true
            index <- index - 1
        next

    member this.getOffsetAddr(index: int): string =
        offsetAddrs.[index]

    member this.updateFingerTableItem(index: int, nodeIdentifier: string) =
        fingerTable.[index] <- nodeIdentifier

    member this.getIdentifier(): string =
        identifier

    member this.getPredecessor(): string =
        predecessor

    member this.setPredecessor(newPredecessor: string) =
        if predecessor.Equals(newPredecessor) = false then
            predecessor <- newPredecessor
            predecessorCode <- ToolsKit.toBigInteger(predecessor)
            resetFingerTable()

    member this.getSuccessor(): string =
        successor

    member this.setSuccessor(newSuccessor: string) =
        if successor.Equals(newSuccessor) = false then
            successor <- newSuccessor
            successorCode <- ToolsKit.toBigInteger(successor)
            resetFingerTable()

    member this.getIdentifierCode(): BigInteger =
        identifierCode

    member this.getPredecessorCode(): BigInteger =
        predecessorCode

    member this.getSuccessorCode(): BigInteger =
        successorCode

    member this.getStabilizeFlg(): bool =
        stabilizeFlg

    member this.hasStabilized() =
        stabilizeFlg <- false

    member this.getFixFingerTableFlg(): bool =
        fixFingerTableFlg

    member this.stopFixFingerTable() =
        fixFingerTableFlg <- false

    // for test
    member this.getOffsetAddrs(): List<string> =
        offsetAddrs

    member this.getFingerTable(): List<string> =
        fingerTable
