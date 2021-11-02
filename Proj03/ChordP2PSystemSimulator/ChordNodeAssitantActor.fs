module ChordNodeAssitantActor

open Akka.FSharp
open Akka.Cluster.Tools.PublishSubscribe

open ToolsKit
open Msgs
open ChordNodeInfo

type ChordNodeAssitantActor(nodeInfo: ChordNodeInfo) =
    inherit Actor()
    
    let context = Actor.Context
    let mediator = DistributedPubSub.Get(Actor.Context.System).Mediator

    let print(info: string) =
        mediator <! new Send("/user/printer", info, true)

    override this.PreStart() =
        mediator <! new Put(Actor.Context.Self)

    override this.OnReceive message =
        match box message with
        | :? Stabilize as msg ->
            if nodeInfo.getStabilizeFlg() then
                print("Stablize")
                let target = context.ActorSelection(context.Self.Path.Root.ToString() + "/user/" + nodeInfo.getSuccessor())
                let x = (string) (Async.RunSynchronously(target <? new AskPredecessor(), -1))
                if x.Equals("") = false && 
                    ToolsKit.isInScope(ToolsKit.toBigInteger(x), nodeInfo.getIdentifierCode(), nodeInfo.getSuccessorCode(), true, true) then
                    nodeInfo.setSuccessor(x)
                mediator <! new Send("/user/" + nodeInfo.getSuccessor(), new Notify(nodeInfo.getIdentifier()), true)
        | :? FixFingerTable as msg ->
            if nodeInfo.getFixFingerTableFlg() then
                print("Fix Finger Table")
                let mutable index = 0 
                while index <= 159 do
                    let key = nodeInfo.getOffsetAddr(index)
                    let updateContent = (string) (Async.RunSynchronously(context.Parent <? new FindSuccessor(key, true), -1))
                    if updateContent.IndexOf("false") = -1 then
                        let partOne = updateContent.Split(':').[0]
                        nodeInfo.updateFingerTableItem(index, partOne)
                    index <- index + 1

                // for test
                let offsetAddrs = nodeInfo.getOffsetAddrs()
                let fingerTable = nodeInfo.getFingerTable()
                index <- 0
        | _ -> printfn "%s gets unknown message" Actor.Context.Self.Path.Name