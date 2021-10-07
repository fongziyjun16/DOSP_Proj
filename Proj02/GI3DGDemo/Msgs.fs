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

type StartRumor =
    struct
    end

type Rumor = 
    struct
    end

type GetRumor =
    struct
        val NAME: string
        new (name: string) ={
            NAME = name
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