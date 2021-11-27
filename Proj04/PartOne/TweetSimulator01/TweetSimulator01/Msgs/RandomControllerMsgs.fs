namespace Msgs

open System.Collections.Generic
open System.Dynamic
open System.Text

type RegisterTest =
    struct
    end

type LoginLogoutTest =
    struct
    end

type CLientPostTest = 
    struct
    end

type QueryTest = 
    struct
    end
    
type StartSimulationWithZipf =
    struct
    end
    
type StatisticsStatusEntity =
    struct
        val ID: int
        val NAME: string
        val NUMBER_OF_FOLLOWER: string
        val POST_RATE: double
        new (id: int, name: string, numberOfFollower: string, postRate: double) = {
            ID = id
            NAME = name
            NUMBER_OF_FOLLOWER = numberOfFollower
            POST_RATE = postRate
        }
        
        member this.toString(): string =
            let str = new StringBuilder()
            str.Append("ID: " + this.ID.ToString()) |> ignore
            str.Append("   ") |> ignore
            str.Append("name: " + this.NAME) |> ignore
            str.Append("   ") |> ignore
            str.Append("number of follower: " + this.NUMBER_OF_FOLLOWER) |> ignore
            str.Append("   ") |> ignore
            str.Append("post rate: " + this.POST_RATE.ToString() + " %") |> ignore
            str.ToString()
    end
    
type StatisticsStatusResult =
    struct
        val CLIENTS_STATUS: List<StatisticsStatusEntity>
        new (clientsStatus: List<StatisticsStatusEntity>) = {
            CLIENTS_STATUS = clientsStatus
        }
    end