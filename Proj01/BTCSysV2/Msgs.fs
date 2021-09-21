// defines various kinds of messages

module Msgs

// conntent needed to be printed -> printer
type PrintingInfo =
    struct
        val FROM: string
        val CONTENT: string
        new (from: string, content: string) = {
            FROM = from;
            CONTENT  = content;
        }
    end

type FoundOneResult =
    struct
        val RESULT: string
        val FROM: string
        new (result: string, from: string) = {
            RESULT = result;
            FROM = from
        }
    end

type OuterPrintingInfo = 
    struct
        val FROM: string
        val CONTENT: string
        new (from: string, content: string) = {
            FROM = from;
            CONTENT  = content;
        }
    end

type GetSuffixLength =
    struct
        val FROM: int
        new (from: int) = {
            FROM = from
        }
    end

type StartComputing = 
    struct
        val PREFIX: string
        val NUMBEROFZEROS: int
        new (prefix: string, numberOfZeros: int) = {
            PREFIX = prefix;
            NUMBEROFZEROS = numberOfZeros
        }
    end

type OuterStartComputing =
    struct
        val PREFIX: string
        val NUMBEROFZEROS: int
        new (prefix: string, numberOfZeros: int) = {
            PREFIX = prefix;
            NUMBEROFZEROS = numberOfZeros
        }
    end

type OneTuple = 
    struct
        val COIN: string
        val SHA256: string
        val LEADINGZEROS: int
        new (coin: string, sha256: string) = {
            COIN = coin;
            SHA256 = sha256;
            LEADINGZEROS = 0
        }
        new (coin: string, sha256: string, leadingZeros: int) = {
            COIN = coin;
            SHA256 = sha256;
            LEADINGZEROS = leadingZeros
        }
    end