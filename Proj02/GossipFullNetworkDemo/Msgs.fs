module Msgs

type StartRumor=
    struct
    end

type Rumor = 
    struct
        val CONTENT: string
        new (content: string) = {
            CONTENT = content;
        }
    end

type EndRumor = 
    struct
    end

type RumorCounter =
    struct
        val FROM: string
        val DEST: string
        val COUNTER: int
        new (from: string, dest: string,counter: int) = {
            FROM = from;
            DEST = dest;
            COUNTER = counter
        }
    end

type SendRumor = 
    struct
    end

type GetRumor = 
    struct
    end

type AskStopFlg =
    struct
        val STOP_FLG: bool
        new (stopFlg: bool) = {
            STOP_FLG = stopFlg
        }
    end