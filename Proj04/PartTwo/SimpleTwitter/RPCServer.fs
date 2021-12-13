namespace SimpleTwitter

open System
open WebSharper

module RPCServer =

    [<Rpc>]
    let SignUpProcess(username: string, password: string): bool =
        let newAccount = new Account(username, password)
        let flag = DBOperator.accountDAO.insert(newAccount)
        if flag then
            true
        else
            false
    
    [<Rpc>]
    let SignInProcess(username: string, password: string) =
        async {
            let account = DBOperator.accountDAO.getAccountByUsername(username)
            if account.ID = -1 || account.PASSWORD <> password then
                let res: LoginVerification = {
                    loginSign = false
                    token = ""
                }
                return Json.Serialize res
            else
                let newToken = Guid.NewGuid().ToString()
                let res: LoginVerification = {
                    loginSign = true
                    token = newToken
                }
                RealTimeServer.addNewUsernameToken(username, newToken)
                return Json.Serialize res
        }
    
    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }
