module Utilities

open System
open System.Collections.Generic 
open System.Text.RegularExpressions

//TODO: Move to another module
type Token = {
    raw : string option
    refresh: string option
    expiration: string option
    }

let (|Match|_|) pattern input =
    let m = Regex.Match(input, pattern) in
    if m.Success then Some (List.tail [ for g in m.Groups -> g.Value ]) else None

let regexJsonValue key = 
    let r = sprintf "\"%s\"[ :]+((?=\\[)\\[[^]]*\\]|(?=\\{)\\{[^\\}]*\\}|\\\"[^\"]*\\\")" key //"x"[ :]+((?=\[)\[[^]]*\]|(?=\{)\{[^\}]*\}|\"[^"]*\")
    r

let getJsonStringValue text value = 
    match text with
        | Match (regexJsonValue value) result -> Some(result.Head.Replace("\"",""))
        | _ -> None

let getJsonObjectValue text value = 
    match text with
        | Match (regexJsonValue value) result -> Some(result.Head)
        | _ -> None

let getEncodingPage contentType =
    match contentType.ToString().ToLower() with
    | "utf8" | "utf-8" | "utf16" | "utf-16" -> Some(contentType)
    | _ ->
        let pattern = ".*?charset=([^\"']+)"
        let m = Regex.Match(contentType, pattern) in
        let l = List.tail [ for g in m.Groups -> g.Value ] 
        if m.Success then Some ( l.Head) else None

//meomize pattern to cache functions
let memoize f =
    let dict = Dictionary<_, _>()
    fun x ->
        match dict.TryGetValue(x) with
        | true, res -> res
        | false, _ -> 
            let res = f x
            dict.Add(x, res)
            res

let (|Prefix|_|) (p:string) (s:string) =
    if s.StartsWith(p) then
        Some(s.Substring(p.Length))
    else
        None

//It is important to note that neither of these methods encodes the RFC 2396 unreserved characters which includes -_.!~*'() 
//So if you need these encoded then you will have to manually encode them.

let urlEncode value =
    Uri.EscapeDataString value

// Adds an element to a list which may be none
let append item listOption =
    match listOption with
    | None -> Some([item])
    | Some(existingList) -> Some(existingList@[item])

let transform listOption =
        Some(listOption)

let keyValueStringToDictionary token = 
        let decodeAndSplit item =
            let claims = Uri.UnescapeDataString(item).Split [|'='|]
            (claims.[0],claims.[1])
        match String.IsNullOrEmpty (token) with
            | true -> None
            | _ ->
                let claims = token.Split [|'&'|] |> Seq.map (fun itm -> decodeAndSplit(itm))
                Some (Dictionary<string,string>(claims |> Map.ofSeq) :> IDictionary<string,string>)
