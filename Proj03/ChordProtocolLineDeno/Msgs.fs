module Msgs

// Communicate
type ContextInfo = 
    struct
        val PREDECESSOR: string
        val SUCCESSOR: string
        new (predecessor: string, successor: string) = {
            PREDECESSOR = predecessor;
            SUCCESSOR = successor;
        }
    end

type SetPredecessor = 
    struct
        val PREDECESSOR: string
        new (predecessor: string) = {
            PREDECESSOR = predecessor
        }
    end

type SetSuccessor = 
    struct
        val SUCCESSOR: string
        new (successor: string) = {
            SUCCESSOR = successor
        }
    end

type ReportTimes = 
    struct
        val TIMES: int
        val FROM: string
        new (times: int, from: string) = {
            TIMES = times;
            FROM = from;
        }
    end

type PrintAvgHops =
    struct
        val AVGHOPS: double
        new (avgHops: double) = {
            AVGHOPS = avgHops;
        }
    end

// to Chord Node
type AddNewResource =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type StartFindResource =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name
        }
    end

type CheckResource(name: string, from: string) =
    let NAME = name
    let FROM = from
    let mutable TIMES = 0

    member this.IncrTimes() =
        TIMES <- TIMES + 1

    member this.GetTimes() =
        TIMES

    member this.GetName() =
        NAME

    member this.GetFrom() =
        FROM


type GetResource = 
    struct
        val NAME: string
        val LOCATION: string
        val TIMES: int
        new (name: string, location: string, times: int) = {
            NAME = name;
            LOCATION = location;
            TIMES = times;
        }
    end

type FindingRequest = 
    struct
    end

// to Coordinator
type AddNewChordNode = 
    struct
        val IP: string
        val ENCODE: string
        new (ip: string, encode: string) = {
            IP = ip;
            ENCODE = encode;
        }
    end

type AssignNewResource = 
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end
    


