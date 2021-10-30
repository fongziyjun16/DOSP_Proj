module ToolsKit

open System
open System.Text
open System.Numerics
open System.Globalization
open System.Security.Cryptography
open System.Collections.Generic

type ToolsKit() =
    
    static let random = new Random()
    static let identifierSet = new HashSet<string>()
    static let recorder = new SortedDictionary<string, string>()

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

    // generate nodes identifier randomly
    static member generateNodeIdentifier(): string = 
        let mutable identifier = generateOndNodeIdentifier()
        while identifierSet.Contains(identifier) do
            identifier <- generateOndNodeIdentifier()
        identifierSet.Add(identifier) |> ignore
        identifier

    static member getMIN(): BigInteger =
        BigInteger.Parse("0" + new String('0', 40), NumberStyles.HexNumber)

    static member getMAX(): BigInteger =
        BigInteger.Parse("0" + new String('F', 40), NumberStyles.HexNumber)
                   
    static member addRecord(identifier: string) =
        recorder.Add(ToolsKit.encodeBySHA1(identifier), identifier)

    static member getCorrectIdentifiers(): Dictionary<string, Tuple<string, string>> =
        let identifiers = recorder.Values |> List<string>
        let table = new Dictionary<string, Tuple<string, string>>()
        if identifiers.Count = 1 then
            table.Add(identifiers.[0], new Tuple<string, string>(identifiers.[0], identifiers.[0]))
        else
            for i in 0 .. identifiers.Count - 1 do
                if i = 0 then
                    table.Add(identifiers.[i], new Tuple<string, string>(identifiers.[identifiers.Count - 1], identifiers.[1])) |> ignore
                else if i = identifiers.Count - 1 then
                    table.Add(identifiers.[i], new Tuple<string, string>(identifiers.[i - 1], identifiers.[0])) |> ignore
                else
                    table.Add(identifiers.[i], new Tuple<string, string>(identifiers.[i - 1], identifiers.[i + 1])) |> ignore
        table
            
