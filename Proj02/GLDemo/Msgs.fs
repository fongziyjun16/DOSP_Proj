module Msgs

type StartRumor =
    struct
        val MOTIVATED: bool
        new (motivated: bool) = {
            MOTIVATED = motivated
        }
    end

type Rumor = 
    struct
        val FROM: int
        new (from: int) ={
            FROM = from
        }    
    end

type GetRumor =
    struct
        val ID: int
        new (id: int) ={
            ID = id
        }
    end

type GetSwitch =
    struct
    end

type SetSwitch =
    struct
        val SWITCH: bool
        new (switch: bool) = {
            SWITCH = switch
        }
    end

type AllStop =
    struct
    end

type IAmDone =
    struct
        val ID: int
        new (id: int) ={
            ID = id
        }
    end

type Motivation =
    struct
    end