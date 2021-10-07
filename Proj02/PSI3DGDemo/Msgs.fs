module Msgs

type Structure =
    struct
        val LENGTH: int
        val WIDTH: int
        val HEIGHT: int
        new (length: int, width: int, height: int) = {
            LENGTH = length;
            WIDTH = width;
            HEIGHT = height
        }
    end

type Position = 
    struct
        val X: int
        val Y: int
        val Z: int
        new (x: int, y: int, z: int) = {
            X = x;
            Y = y;
            Z = z
        }
    end

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

type OneRoundGet =
    struct
    end

type Termination =
    struct
        val NAME: string
        new (name: string) ={
            NAME = name
        }
    end
