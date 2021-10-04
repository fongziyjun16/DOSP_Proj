module Msgs

type StartRumor = 
    struct
    end

type Rumor =
    struct
        val mutable NO: int
        new (no: int) = {
            NO = no;
        }

        member x.SetRumorNO(rumorNO: int) =
            x.NO <- rumorNO
    end

type NumberOfActorGetRumor =
    struct
        val mutable NUMBER: int
        new (number: int) = {
            NUMBER = number;
        }

        member x.SetNumber(number: int) =
            x.NUMBER <- number
    end

type ReqNewRumorNO = 
    struct
    end

type SingleActorStopSendingFlg =
    struct
    end

type EndRumor =
    struct
    end

type AllGetRumor =
    struct
    end

type ReqNewRoundDissemination =
    struct
        val NO: int
        new (no: int) = {
            NO = no
        }
    end
