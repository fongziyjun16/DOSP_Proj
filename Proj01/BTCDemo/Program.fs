// simulate more than one actor calculate BTC hash value
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Event

open ComputationWork
open Msgs

let sys = System.create "sys" (Configuration.load())

let eventStation = sys.EventStream
    
type worker() =
    inherit Actor()

    let mission = new ComputingMission()

    member x.GetName() =
        x.Self.Path.Name

    override x.OnReceive message = 
        match message with
        | :? missionInfo as msg ->
            printfn "%s starts computing" (x.GetName())
            let result = mission.findTheHash(msg.PREFIX, msg.NUMBEROFZERO)
            if result.STAT then
                printfn "%s get results: \"%s\"" (x.GetName()) result.RESULT
            else
                printfn "%s stops computing" (x.GetName())
        | _ -> failwith "unknow message"

[<EntryPoint>]
let main argv =

    let msg = new missionInfo("yingjie.chen", 6)

    let actors = 
        [1 .. 10]
        |> List.map(
            fun id -> 
                sys.ActorOf(Props(typedefof<worker>), "actor" + id.ToString()))

    for actor in actors do
        eventStation.Subscribe(actor, typedefof<missionInfo>) |> ignore

    eventStation.Publish(msg)

    System.Console.Read() |> ignore
    0