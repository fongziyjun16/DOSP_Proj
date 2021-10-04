module Msgs

type StartRumor = 
    struct
    end

type Rumor =
    struct
    end

type GetSwitch = 
    struct
    end

type SetSwitch = 
    struct
        val VALUE: bool
        new (value: bool) = {
            VALUE = value
        }
    end

type ReqNewRumorNO = 
    struct
    end

type EndRumor =
    struct
    end

type AllGetRumor =
    struct
    end
