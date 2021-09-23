open System.Diagnostics

let time f = 
    let proc = Process.GetCurrentProcess()
    let cpu_time_stamp = proc.TotalProcessorTime
    let timer = new Stopwatch()
    timer.Start()
    try
        f()
    finally
        let cpu_time = (proc.TotalProcessorTime-cpu_time_stamp).TotalMilliseconds
        printfn "CPU time = %dms" (int64 cpu_time)
        printfn "Absolute time = %dms" timer.ElapsedMilliseconds

let rec loop n f x =
    if n > 0 then
        f x |> ignore
        loop (n-1) f x

time (fun () -> loop 1000000 List.sum [1..100])
