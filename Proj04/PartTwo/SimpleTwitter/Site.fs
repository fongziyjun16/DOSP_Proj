namespace SimpleTwitter

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Html
open WebSharper.UI.Server
open WebSharper.AspNetCore.WebSocket

type EndPoint =
    | [<EndPoint "/">] Account
    | [<EndPoint "/main">] Main


module Site =

    [<Website>]
    let Main =
        Application.MultiPage (fun (ctx: Context<_>) endpoint ->
            match endpoint with
            | EndPoint.Account ->
                Content.Page(
                    Templates.AccountTemplate()
                        .Body(client <@ AccountPageProcess.formProcess() @>)
                        .Doc()
                )
            | EndPoint.Main ->
                let buildEndPoint(url: string): WebSocketEndpoint<S2CMessage, C2SMessage> =
                    WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)
                let ep = buildEndPoint(ctx.RequestUri.ToString())
                Content.Page(
                    Templates.MainTemplate()
                        .Body(client <@ MainPageProcess.Process(ep) @>)
                        .Doc()
                )
        )
