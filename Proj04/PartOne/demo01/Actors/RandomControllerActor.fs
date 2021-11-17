namespace Actor

open Akka.FSharp

type RandomControllerActor(numberOfClients: int) =
    inherit Actor()

    override this.OnReceive message =
        match box message with
        | :? string as msg ->
            printfn "[%s]:[%s]" (Actor.Context.Sender.Path.ToStringWithAddress()) msg
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name
