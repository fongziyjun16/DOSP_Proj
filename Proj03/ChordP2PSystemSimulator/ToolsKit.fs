module ToolsKit

open System
open System.Linq
open System.Text
open System.Numerics
open System.Globalization
open System.Security.Cryptography
open System.Collections.Generic

type ToolsKit() =
    
    static let random = new Random()
    static let identifierSet = new HashSet<string>()
    static let nodesIdentifierInfo = new SortedDictionary<string, string>()
    static let nodesContextInfo = new SortedDictionary<string, string>()
    static let mutable isCompleteStructure = false

    static let generateOndNodeIdentifier(): string = 
        // the format of node identifier is "IP:Port"
        let port = (random.Next(0, 65536)).ToString()
        let mutable nodeIdentifier = ""
        for j = 1 to 4 do
            nodeIdentifier <- nodeIdentifier + random.Next(0, 256).ToString()
            if j <> 4 then
                nodeIdentifier <- nodeIdentifier + "_" 
        nodeIdentifier <- (nodeIdentifier + "_" + port.ToString())
        nodeIdentifier

    // encode key by SHA1
    static member encodeBySHA1(key: string): string = 
        "0" + BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key))).Replace("-", String.Empty)

    // get BigInteger
    static member toBigInteger(key: string): BigInteger =
        BigInteger.Parse("0" + ToolsKit.encodeBySHA1(key), NumberStyles.HexNumber)

    static member getMIN(): BigInteger =
        BigInteger.Parse("0" + new String('0', 40), NumberStyles.HexNumber)

    static member getMAX(): BigInteger =
        BigInteger.Parse("0" + new String('F', 40), NumberStyles.HexNumber)

    // generate nodes identifier randomly
    static member generateNodeIdentifier(): string = 
        let mutable identifier = generateOndNodeIdentifier()
        while identifierSet.Contains(identifier) do
            identifier <- generateOndNodeIdentifier()
        identifierSet.Add(identifier) |> ignore
        identifier

    static member generateOneRandomResource(): string =
        let random = new Random()
        let letters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        let randomLength = random.Next(10, 41)
        let randomResource = new StringBuilder()
        for i in 1 .. randomLength do
            randomResource.Append(letters.[random.Next(0, letters.Length)]) |> ignore
        randomResource.ToString()

    static member isInScope(target: BigInteger, left: BigInteger, right: BigInteger, leftSwitch: bool, rightSwitch: bool): bool =
        let mutable flg = false
        if left.CompareTo(right) < 0 then
            if target.CompareTo(left) >= 0 && target.CompareTo(right) <= 0 then
                flg <- true
        else
            if target.CompareTo(left) >= 0 && target.CompareTo(ToolsKit.getMAX()) <= 0 || 
                target.CompareTo(ToolsKit.getMIN()) >= 0 && target.CompareTo(right) <= 0 then
                flg <- true

        if leftSwitch = true && target.CompareTo(left) = 0 || rightSwitch = true && target.CompareTo(right) = 0 then
            flg <- false

        flg

    static member addNodeIdentifierEntry(identifier: string) =
        nodesIdentifierInfo.Add(ToolsKit.encodeBySHA1(identifier), identifier)

    static member getNodeIndentifiers(): SortedDictionary<string, string> =
        nodesIdentifierInfo

    static member getNodesContextInfo(): SortedDictionary<string, string> =
        if nodesContextInfo.Count = 0 then 
            let identifiers = nodesIdentifierInfo.Values.ToList()
            let number = identifiers.Count
            for i in 0 .. number - 1 do
                if i = 0 then
                    nodesContextInfo.Add(identifiers.[i], identifiers.[number - 1] + ":" + identifiers.[number |> fun number -> if number = 1 then 0 else i + 1])
                else if i = number - 1 then
                    nodesContextInfo.Add(identifiers.[i], identifiers.[i - 1] + ":" + identifiers.[0])
                else
                    nodesContextInfo.Add(identifiers.[i], identifiers.[i - 1] + ":" + identifiers.[i + 1])
        nodesContextInfo

    static member calculatePowerOf2(i: int): string =
        let test = new StringBuilder("1" + new String('0', i))
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

    static member checkCompleteStructure() = 
        isCompleteStructure

    static member builtStructure() =
        isCompleteStructure <- true