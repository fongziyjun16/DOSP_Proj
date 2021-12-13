namespace SimpleTwitter

open WebSharper
open WebSharper.UI.Templating

[<JavaScript>]
module Templates =

    type AccountTemplate = Template<".\\templates\\Account.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type MainTemplate = Template<".\\templates\\Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>
