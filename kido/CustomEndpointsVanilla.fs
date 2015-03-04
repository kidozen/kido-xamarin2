namespace Kidozen

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open KzApplication
open System.Runtime.InteropServices

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.IO
open System.Linq
open System.Collections.Generic

type CustomEndpoint (name, identity:Identity) = 
    let name = name
    let baseurl =(getJsonStringValue (identity.config) "ssc" ).Value
    member this.identity = identity

    member private this.ProcessResponse result =
        let content = result.EntityBody.Value
        match result.StatusCode with
                | 200 ->
                    let jobj = JObject.Parse(content)
                    let error = jobj.["err"]
                    let data = jobj.["result"]
                    match data with
                        | null -> match error  with
                                    | null -> raise ( new Exception ("Unknown error."))
                                    | _ -> raise ( new Exception (error.ToString()))
                        | _ -> data.ToString()
                | _ -> raise ( new Exception (result.EntityBody.Value))

    member this.Execute<'a>() =
        let url = sprintf "%s/%s" baseurl name
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseAsync                             
            return  JsonConvert.DeserializeObject<'a>( this.ProcessResponse result )                
            }
        service |> Async.StartAsTask

    member this.Execute() =
        let url = sprintf "%s/%s" baseurl name
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseAsync                             
            return this.ProcessResponse result                
            }
        service |> Async.StartAsTask


    member this.Execute<'a>(parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let url = sprintf "%s/%s" baseurl name
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withHeader (ContentType "application/json")
                            |> withBody paramsAsString
                            |> getResponseAsync                             
            return  JsonConvert.DeserializeObject<'a>( this.ProcessResponse result )                
            }
        service |> Async.StartAsTask

    member this.Execute(parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let url = sprintf "%s/%s" baseurl name
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withHeader (ContentType "application/json")
                            |> withBody paramsAsString
                            |> getResponseAsync                             
            return this.ProcessResponse result                
            }
        service |> Async.StartAsTask


