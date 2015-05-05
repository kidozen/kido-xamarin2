namespace Kidozen

open System
open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic
open Serialization
open HttpClient
open Utilities
open identityproviders
open KzApplication
open System.Runtime.InteropServices

type Analytics (identity:Identity) = 
    member this.identity = identity
    member this.SaveSession (session) =
        let baseurl =(getJsonStringValue (identity.config) "url" ).Value 
        let logep = baseurl + "api/v3/logging/events?level=1"
        let body = JSONSerializer.toString (session)
        let service =  async {
            let! validAuthToken = validateToken this.identity
            let! result = createRequest HttpMethod.Post logep  
                            |> withHeader (Authorization validAuthToken.rawToken) 
                            |> withHeader (ContentType "application/json")  
                            |> withHeader (Accept "application/json") 
                            |> withBody body
                            |> getResponseAsync 
            return 
                match result.StatusCode with
                    | 201 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        service |> Async.StartAsTask

    member this.SaveSession (session) =
        let baseurl =(getJsonStringValue (identity.config) "url" ).Value 
        let logep = baseurl + "api/v3/logging/events?level=1"
        let service =  async {
            let! validAuthToken = validateToken this.identity
            let! result = createRequest HttpMethod.Post logep  
                            |> withHeader (Authorization validAuthToken.rawToken) 
                            |> withHeader (ContentType "application/json")  
                            |> withHeader (Accept "application/json") 
                            |> withBody session
                            |> getResponseAsync 
            return 
                match result.StatusCode with
                    | 201 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        service |> Async.StartAsTask
    