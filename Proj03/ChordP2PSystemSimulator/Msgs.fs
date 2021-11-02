module Msgs

open System.Numerics

type Join =
    struct
        val ID: string
        new (id: string) = {
            ID = id;
        }
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

type AskPredecessor =
    struct
    end

type FindSuccessor =
    struct
        val KEY: string
        val ENCODED: bool
        new (key: string, encoded: bool) = {
            KEY = key;
            ENCODED = encoded;
        }
    end

type StartLookupMission = 
    struct
    end

type PreLookup =
    struct
    end

type Lookup(key: string, publisher: string) =
    
    let mutable steps = 0

    member this.incrSteps() =
        steps <- steps + 1

    member this.getKey(): string =
        key

    member this.getPublisher(): string =
        publisher

    member this.getSteps(): int = 
        steps

type FoundResource =
    struct
        val KEY: string
        val STEPS: int
        val PUBLISHER: string
        new (key: string, steps: int, publisher: string) = {
            KEY= key;
            STEPS = steps;
            PUBLISHER = publisher;
        }
    end

type AskContextInfo =
    struct
    end

type PrintContextInfo =
    struct
    end

type CheckChordStructure =
    struct
    end

type StopStabilize =
    struct
    end

type StopFixFingerTable =
    struct
    end