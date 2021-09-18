open System

let SHA256AnyString2Hex (str : string) =
    Security.Cryptography.SHA256.Create().ComputeHash(Text.Encoding.Default.GetBytes str)
    |> Array.map (fun (x : byte) -> String.Format("{0:X2}", x))
    |> String.concat String.Empty

let isValid (str : string, zeros : int) =
    let zerosSB = new Text.StringBuilder();
    for i in 1 .. zeros do
        zerosSB.Append('0')
    let pattern = Text.RegularExpressions.Regex("^" + zerosSB.ToString() + ".*$");
    pattern.IsMatch(str)

let recorderIncrement (recorder : Collections.Generic.List<int>) =
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

let buildOrign (prefix : string, recorder : Collections.Generic.List<int>) =
    let orgPrefix = new Text.StringBuilder(prefix)
    for charIntValue in recorder do
        orgPrefix.Append(Convert.ToChar charIntValue)
    orgPrefix.ToString()

let findTheHash (prefix : string, zeros : int) = 
    let recorder = new Collections.Generic.List<int>()
    recorder.Add(32);

    let mutable testValue = buildOrign(prefix, recorder)
    while isValid(SHA256AnyString2Hex(testValue), zeros) = false do
        printfn "invalid hash value: %s" testValue
        recorderIncrement recorder
        testValue <- (buildOrign(prefix, recorder))
    printfn "valid hash value: \"%s\"" testValue
    
[<EntryPoint>]
let main argv =
    findTheHash("wei.he", 5)
    0 
