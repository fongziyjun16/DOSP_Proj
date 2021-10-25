module Tools

open System
open System.Linq
open System.Text
open System.Security.Cryptography
open System.Collections.Generic

let getSHA1(key: string): string = 
    BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key))).Replace("-", String.Empty)

let random = new Random()

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

let buildNodeNameByIP(ip: string): string = 
    let parts = ip.Split('.')
    let mutable name = ""
    for i in 0 .. parts.Length - 1 do
        if i = parts.Length - 1 then
            name <- name + parts.[i]
        else
            name <- name + parts.[i] + "_"
    name

let mutable table = new SortedDictionary<string, string>()
let mutable keys = new List<string>()
let mutable ips = new List<string>()

let binarySearch(key:string, dir: bool): string = // dir false -- left ; true -- right
    let mutable left = 0
    let mutable right = keys.Count - 1

    while left < right - 1 do
        let mid = left + (right - left) / 2
        if keys.[mid].CompareTo(key) > 0 then
            right <- mid
        else
            left <- mid
    
    if dir then // true right
        keys.[right]
    else // false left
        keys.[left]

let findPredecessor(key: string): string =
    if key.CompareTo(keys.[0]) < 0 || key.CompareTo(keys.[keys.Count - 1]) > 0 then
        keys.[keys.Count - 1]
    else binarySearch(key, false)

let findSuccessor(key: string): string = 
    if key.CompareTo(keys.[0]) < 0 || key.CompareTo(keys.[keys.Count - 1]) > 0 then
        keys.[0]
    else binarySearch(key, true)

let mutable resourceList = new List<string>()

