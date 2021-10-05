﻿module Msgs

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