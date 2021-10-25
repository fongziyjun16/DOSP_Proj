module Msgs

// to Resource Manager
type AssignAllResources = 
    struct
    end

// to Chord Node
type ReceiveResource =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type ContextInfo = 
    struct
        val PREDECESSOR: string
        val SUCCESSOR: string
        new (predecessor: string, successor: string) = {
            PREDECESSOR = predecessor;
            SUCCESSOR = successor;
        }
    end

type AddNewResource =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name
        }
    end

type IntervalRequest =
    struct
    end

type CheckResource(resource: string, from: string) =
    let mutable step = 0

    member this.GetResource() = 
        resource

    member this.GetFrom() =
        from

    member this.GetStep() =
        step

    member this.IncrStep() =
        step <- step + 1

// to Chord Manager
type AddNewChordNode = 
    struct
        val SHA1CODE: string
        val IP: string
        new (sha1Code: string, ip: string) = {
            SHA1CODE = sha1Code;
            IP = ip;
        }
    end

type NotifyNodeContext =
    struct
    end

type NotifyNodeRequest =
    struct
    end

type NodeFoundResource = 
    struct
        val FROM: string
        val STEPS: int
        new (from: string, steps: int) = {
            FROM = from;
            STEPS = steps;
        }
    end


