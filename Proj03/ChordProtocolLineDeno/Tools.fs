module Tools

open System
open System.Text
open System.Linq
open System.Security.Cryptography
open System.Collections.Generic

let random = new Random()
let letters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWUXYZabcdefghijklmnopqrstuvwxyz"

let getRandomIPs(number: int): List<string> = 
    let randomIPSet = new HashSet<string>()
    
    while randomIPSet.Count <> number do
        let mutable ip = ""
        for j = 1 to 4 do
            ip <- ip + random.Next(0, 256).ToString()
            if j <> 4 then
                ip <- ip + "." 
        randomIPSet.Add(ip) |> ignore

    randomIPSet.ToList()

let getRandomResource(number: int): List<string> =
    let randomResourceSet = new HashSet<string>()
    
    while randomResourceSet.Count <> number do
        let randomLength = random.Next(10, 33)
        let randomResource = new StringBuilder()
        for i in 1 .. randomLength do
            randomResource.Append(letters.[random.Next(letters.Length)]) |> ignore
        randomResourceSet.Add(randomResource.ToString()) |> ignore

    randomResourceSet |> List<string>

let getSHA1(key: string): string = 
    BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key))).Replace("-", String.Empty)

let buildChordNodeName(ip: string): string = 
    let parts = ip.Split('.')
    let mutable name = ""
    for i in 0 .. parts.Length do
        if i = parts.Length - 1 then
            name <- name + parts.[i]
        else
            name <- name + parts.[i] + "_"
    name