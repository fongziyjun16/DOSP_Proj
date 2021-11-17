module ToolsKit

open System
open System.Text
open System.Collections.Generic

type Tools() =
    
    static let random = new Random()

    static let clientList = new List<string>()

    static member getRandomString(small: int, big: int): string =
        let letters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        let randomLength = random.Next(small, big)
        let randomResource = new StringBuilder()
        for i in 1 .. randomLength do
            randomResource.Append(letters.[random.Next(0, letters.Length)]) |> ignore
        randomResource.ToString()

    static member addNewClient(name: string) =
        clientList.Add(name) |> ignore

    static member getRandomClient(): string =
        clientList.[random.Next(clientList.Count)]