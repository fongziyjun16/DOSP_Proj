module Msgs

type Rumor =
    struct
        val S: double
        val W: double
        new (s: double, w: double) = {
            S = s;
            W = w
        }
    end