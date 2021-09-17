module ComputationWork

open System

open Msgs

type ComputingMission() = 

    static let mutable flg = true

    member this.SHA256AnyString2Hex (str : string) =
        Security.Cryptography.SHA256.Create().ComputeHash(Text.Encoding.Default.GetBytes str)
        |> Array.map (fun (x : byte) -> String.Format("{0:X2}", x))
        |> String.concat String.Empty

    member this.isValid (str : string, zeros : int) =
        let zerosSB = new Text.StringBuilder();
        for i in 1 .. zeros do
            zerosSB.Append('0') |> ignore
        let pattern = Text.RegularExpressions.Regex("^" + zerosSB.ToString() + ".*$");
        pattern.IsMatch(str)

    member this.recorderIncrement (recorder : Collections.Generic.List<int>) =
        let mutable extra = 0;
        recorder.[recorder.Count - 1] <- recorder.[recorder.Count - 1] + 1
        if recorder.[recorder.Count - 1] = 127 then
            extra <- 1
            recorder.[recorder.Count - 1] <- 32
    
        let mutable counter = recorder.Count - 2
        while extra <> 0 && counter >= 0 do
            recorder.[counter] <- recorder.[counter] + extra
            extra <- 0
            if recorder.[counter] = 127 then
                extra <- 1
                recorder.[counter] <- 32
            counter <- counter - 1

        if extra = 1 then
            recorder.Insert(0, 32)

    member this.buildOrign (prefix : string, recorder : Collections.Generic.List<int>) =
        let orgPrefix = new Text.StringBuilder(prefix)
        for charIntValue in recorder do
            orgPrefix.Append(Convert.ToChar charIntValue) |> ignore
        orgPrefix.ToString()
    
    member this.findTheHash (prefix : string, zeros : int) = 
        let recorder = new Collections.Generic.List<int>()
        recorder.Add(32);

        let mutable testValue = this.buildOrign(prefix, recorder)
        while flg && this.isValid(this.SHA256AnyString2Hex(testValue), zeros) = false do
            this.recorderIncrement recorder
            testValue <- (this.buildOrign(prefix, recorder))
        // printfn "valid hash value: \"%s\"" testValue
        let mutable resultFlg = flg
        if flg then 
            flg <- false
            resultFlg <- true
        new missionResult(testValue, resultFlg)
        
    

