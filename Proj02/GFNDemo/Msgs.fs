module Msgs

type StartRumor =
    struct
    end

type Rumor = 
    struct
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