namespace Kidozen

open System

open System.Collections.Generic
open Newtonsoft
open Newtonsoft.Json

type Mail() = 
    [<JsonProperty("to")>]
    member val To = String.Empty with get,set
    [<JsonProperty("from")>]
    member val From = String.Empty with get, set
    [<JsonProperty("bodyHtml")>]
    member val HtmlBody = String.Empty with get, set
    [<JsonProperty("bodyText")>]
    member val TextBody = String.Empty with get, set
    [<JsonProperty("subject")>]
    member val Subject = String.Empty with get, set
    [<JsonProperty("attachments")>]
    member val Attachments =  new List<string>() with get,set
