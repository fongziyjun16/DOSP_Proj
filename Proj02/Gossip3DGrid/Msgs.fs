module Msgs

type GridStructure =
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

type WorkerPosition =
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

type StartRumor=
    struct
    end

type Rumor =
    struct
    end

type EndRumor =
    struct
    end
