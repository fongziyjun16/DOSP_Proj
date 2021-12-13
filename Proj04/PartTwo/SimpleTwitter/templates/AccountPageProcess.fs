namespace SimpleTwitter

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI

[<JavaScript>]
module AccountPageProcess =
    
    let formProcess() =
        let state = Var.Create ""
        Templates.AccountTemplate.AccountForm()
            .SignIn(fun e ->
                async {
                    let username = e.Vars.username.Value
                    let password = e.Vars.password.Value
                    if username.Length > 0 && password.Length > 0 then
                        let! verification = RPCServer.SignInProcess(username, password)
                        let verificationObj = Json.Parse(verification)
                        // Console.Log(verificationObj.GetJS("loginSign"))
                        let loginResult = verificationObj.GetJS("loginSign") |> Boolean
                        if loginResult.ToString().Equals("true") then
                            JS.Document.Cookie <- "username=" + username
                            JS.Document.Cookie <- "token=" + verificationObj.GetJS("token").ToString()
                            state.Value <- username + " sign in"
                            JS.Window.Location.Href <- JS.Window.Location.Href + "main"
                        else
                            state.Value <- "Incorrect Username or Password"
                    else
                        state.Value <- "Username & Password are REQUIRED"
                }
                |> Async.StartImmediate
            
            )
            .SignUp(fun e ->
                async {
                    let username = e.Vars.username.Value
                    let password = e.Vars.password.Value
                    if username.Length > 0 && password.Length > 0 then 
                        let flag = RPCServer.SignUpProcess(username, password)
                        if flag then
                            state.Value <- "Sign Up Success"
                        else
                            state.Value <- "Username has EXISTED"
                    else
                        state.Value <- "Username & Password are REQUIRED"
                }
                |> Async.StartImmediate
            )
            .operationState(state.View)
            .Doc()