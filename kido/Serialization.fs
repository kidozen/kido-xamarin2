module Serialization

open System
open System.IO
open System.Linq
open System.Collections.Generic
open Newtonsoft
open Newtonsoft.Json
open HttpClient

let serializeJson entity = 
    let value = JsonConvert.SerializeObject entity
    value

let deserializeJson<'a> (json : string) =
    let value = JsonConvert.DeserializeObject<'a>(json)
    value


type JSONSerializer =
    static member toString entity = 
        let value = JsonConvert.SerializeObject entity
        value

    static member deserializeJson<'a> (json : string) =
        let value = JsonConvert.DeserializeObject<'a>(json)
        value

    // TODO: find a better way to do this
    static member toNameValueList entity = 
        let entityAsString = JSONSerializer.toString (entity)
        let list = 
            match entityAsString.Chars(0) with
            | '[' -> JsonConvert.DeserializeObject<List<NameValue>>(entityAsString)
            | _ -> 
                let items = JsonConvert.DeserializeObject<Dictionary<string, string>>(entityAsString).ToList()
                items.Select(fun itm -> {name=itm.Key;value=itm.Value}).ToList()
        list  |>  List.ofSeq    

        