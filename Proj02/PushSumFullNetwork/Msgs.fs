module Msgs

type StartRumor =
    struct
    end

type Rumor =
    struct
        val S: double
        val W: double
        new (s: double, w: double) = {
            S = s;
            W = w
        }
    end

type EndRumor = 
    struct
        val FROM: string
        new (from: string) = {
            FROM = from
        }
    end

type PrintRumor =
    struct
        val FROM: string
        val S: double
        val W: double
        val COUNTER: int
        new (from: string, s: double, w: double, counter: int) = {
            FROM = from;
            S = s;
            W = w;
            COUNTER = counter
        }
    end