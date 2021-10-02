module Msgs

type StartRumor =
    struct
    end

type Rumor = 
    struct
        val NO: int
        new (no: int) = {
            NO = no
        }
    end

type GetRumor =
    struct
    end

type GetRumorNO =
    struct
    end

type GetState =
    struct
        val LAST_RUMOR_MSG_NO: int
        val ALL_GET_RUMOR_FLG: bool
        new (lastRumorMsgNO: int, allGetRumorFlg: bool) = {
            LAST_RUMOR_MSG_NO = lastRumorMsgNO;
            ALL_GET_RUMOR_FLG = allGetRumorFlg
        }
    end

type MotivateRumor =
    struct
    end

type EndRumor = 
    struct
    end

