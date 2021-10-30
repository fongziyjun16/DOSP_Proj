module Msgs
// test use
type Test =
    struct
    end

type PrintContextInfo =
    struct
    end

// real use
type FindSuccessor =
    struct
        val ID: string
        new (id: string) = {
            ID = id;
        }
    end

type Join =
    struct
        val ID: string
        new (id: string) = {
            ID = id;
        }
    end

type AskPredecessor =
    struct
    end

type Stabilize = 
    struct
    end

type Notify =
    struct
        val ID: string
        new (id: string) = {
            ID = id;
        }
    end
    
type FixFingerTable = 
    struct
    end
    
type FoundResource =
    struct
        val STEPS: int
        new (steps: int) = {
            STEPS = steps;
        }
    end
    
type StartMission =
    struct
    end

type Lookup(key: string) =
    
    let mutable steps = 0
    
    member this.incrSteps() = 
        steps <- steps + 1

    member this.getSteps() =
        steps

    member this.getKey() =
        key

type UpdSuccessor =
    struct
        val ID: string
        new (id: string) = {
            ID = id;
        }
    end

type CheckChordStructure=
    struct
    end

type AskNodeContext =
    struct
    end
