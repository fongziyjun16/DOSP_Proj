open System

open GFNSystem
open GLSystem
open G3DGSystem
open GI3DGSystem

open PSFNSystem
open PSLSystem
open PS3DGSystem
open PSI3DGSystem

[<EntryPoint>]
let main argv =

    let topology = argv.[0]
    let algorithm = argv.[1]

    if topology = "full" || topology = "line" then
        let mutable numberOfWorkers = 0
        printf "please input the numberOfWorkers:"
        numberOfWorkers <- Console.ReadLine() |> int

        if algorithm = "gossip" then
            let mutable rumorLimit = 0
            printf "please input the rumor limit timit:"
            rumorLimit <- Console.ReadLine() |> int

            if topology = "full" then
                new GFN(numberOfWorkers, rumorLimit) |> ignore
            else if topology = "line" then
                new GL(numberOfWorkers, rumorLimit) |> ignore

        else if algorithm = "push-sum" then
            if topology = "full" then
                new PSFN(numberOfWorkers) |> ignore
            else if topology = "line" then
                new PSL(numberOfWorkers) |> ignore

    if topology = "imp3D" || topology = "3D" then
        let mutable length = 0
        let mutable width = 0
        let mutable height = 0

        printf "please input the length:"
        length <- Console.ReadLine() |> int
        printf "please input the width:"
        width <- Console.ReadLine() |> int
        printf "please input the height:"
        height <- Console.ReadLine() |> int

        if algorithm = "gossip" then
            let mutable rumorLimit = 0
            printf "please input the rumor limit timit:"
            rumorLimit <- Console.ReadLine() |> int

            if topology = "3D" then
                new G3DG(length, width, height, rumorLimit) |> ignore
            else if topology = "imp3D" then
                new GI3DG(length, width, height, rumorLimit) |> ignore

        else if algorithm = "push-sum" then
            if topology = "3D" then
                new PS3DG(length, width, height) |> ignore
            else if topology = "imp3D" then
                new PSI3DG(length, width, height) |> ignore

    Console.Read() |> ignore
    0 // return an integer exit code