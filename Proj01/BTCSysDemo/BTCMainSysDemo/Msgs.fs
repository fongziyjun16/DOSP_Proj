// define various kinds of message
module Msgs

type FindResult = 
    struct
        val RESULT: string
        val FROM: string
        new (result: string, from: string) = {
            RESULT = result;
            FROM =from
        }
    end

type GetResultState =
    struct
    end

type StartComputation = 
    struct
        val PREFIX: string
        val NUMBEROFZEROS: int
        new (prefix: string, numberOfZeros: int) = {
            PREFIX = prefix;
            NUMBEROFZEROS = numberOfZeros;
        }
    end

type StopComputation =
    struct
    end

type PrintInfo =
    struct
        val SENDER: string
        val CONTENT: string
        new (sender: string, content: string) = {
            SENDER = sender ;
            CONTENT = content
        }
    end

type SetSuffixLength =
    struct
        val NEWVALUE: int
        new (newValue: int) = {
            NEWVALUE = newValue
        }
    end

type IncrSuffixLength =
    struct
        val ORG: int
        val NEWVALUE: int
        val FROM: string
        new (org: int, newValue: int, from: string) = {
            ORG = org;
            NEWVALUE = newValue;
            FROM = from
        }
    end

type GetSuffixLength =
    struct
    end

type SuffixLength =
    struct
        val LENGTH: int
        new (length: int) = { LENGTH = length }
    end

type ConnectionInfo = 
    struct
        val FROM: string // url
        val BUILD: bool
        new (from: string, build: bool) = {
            FROM = from;
            BUILD = build
        }
    end

type OuterStartParas =
    struct
        val PREFIX: string
        val NUMBEROFZEROS: int
        val SUFFIXLENGTH: int
        new (prefix: string, numberOfZeros: int, suffixLength: int) = {
            PREFIX = prefix;
            NUMBEROFZEROS = numberOfZeros;
            SUFFIXLENGTH = suffixLength
        }
    end

type OuterFindResult =
    struct
        val RESULT: string
        val FROM: string
        new (result: string, from: string) = {
            RESULT = result;
            FROM =from
        }
    end

type MainSysConnectorInfo = 
    struct
        val MAINSYSNAME: string
        val IP: string
        val PORT: int
        val CONNECTORNAME: string
        new (mainSysName: string, ip: string, port: int, connectorName: string) = {
            MAINSYSNAME = mainSysName;
            IP = ip;
            PORT = port;
            CONNECTORNAME = connectorName
        }
    end