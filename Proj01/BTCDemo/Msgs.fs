module Msgs

// define different kinds of message

type missionInfo = 
    struct
        val PREFIX: string
        val NUMBEROFZERO: int
        new (prefix: string, numberOfZero: int) = 
            { PREFIX = prefix; NUMBEROFZERO = numberOfZero }
    end

type missionResult = 
    struct
        val RESULT: string
        val STAT: bool
        new (result: string, stat: bool) =
            {RESULT = result ; STAT = stat}
    end

