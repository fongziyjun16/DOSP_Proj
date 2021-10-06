module Msgs

type Start =
    struct
    end

type StartTask =
    struct
    end

type SendOut =
    struct
    end

type CheckIsContinue =
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

type Calculation =
    struct
    end

type Termination =
    struct
        val ID: int
        new (id: int) ={
            ID = id
        }
    end
