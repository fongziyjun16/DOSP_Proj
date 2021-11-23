module ToolsKit

open System
open System.Text
open System.Collections.Generic

type Tools() =
    
    static let random = new Random()

    static let clientList = new List<string>()
    static let hashtagList = new List<string>()

    static member getRegiteredClientNumber(): int =
        clientList.Count

    static member getRandomString(small: int, big: int): string =
        let letters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        let randomLength = random.Next(small, big)
        let randomResource = new StringBuilder()
        for i in 1 .. randomLength do
            randomResource.Append(letters.[random.Next(0, letters.Length)]) |> ignore
        randomResource.ToString()

    static member addNewClient(name: string) =
        clientList.Add(name) |> ignore

    static member getRandomClient(): string =
        clientList.[random.Next(clientList.Count)]

    static member addNewHashtag(name: string) =
        hashtagList.Add(name) |> ignore

    static member getRandomHashtag(): string =
        if hashtagList.Count > 0 then
            hashtagList.[random.Next(hashtagList.Count)]
        else ""

    static member buildPrintingTweet(creator: string, content: string, mentions: List<string>, hashtags: List<string>, retweet: bool): string =
        let tweet = new StringBuilder()
        tweet.Append("Tweet { \n") |> ignore
        tweet.Append("creator: " + creator + "; \n") |> ignore
        tweet.Append("content: " + content + "; \n") |> ignore

        tweet.Append("mentions: [") |> ignore
        for i in 0 .. mentions.Count - 1 do
            if i <> mentions.Count - 1 then
                tweet.Append("@" + mentions.[i] + ", ") |> ignore
            else
                tweet.Append("@" + mentions.[i]) |> ignore
        tweet.Append("]; \n") |> ignore

        tweet.Append("hashtags: [") |> ignore
        for i in 0 .. hashtags.Count - 1 do
            if i <> hashtags.Count - 1 then
                tweet.Append("#" + hashtags.[i] + ", ") |> ignore
            else
                tweet.Append("#" + hashtags.[i]) |> ignore
        tweet.Append("]; \n") |> ignore
        tweet.Append("}") |> ignore

        tweet.ToString()