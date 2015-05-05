namespace Kidozen

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open KzApplication
open System.Runtime.InteropServices

open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic

type Sms (number, identity:Identity) = 
    let baseurl =(getJsonStringValue (identity.config) "sms" ).Value

    member this.number = number
    member this.identity = identity

    member this.Send (message) =
        let service =  async {
            let! validAuthToken = validateToken this.identity
            let! result = createRequest HttpMethod.Post baseurl  
                            |> withHeader (Authorization validAuthToken.rawToken) 
                            |> withHeader (ContentType "application/json")  
                            |> withHeader (Accept "application/json") 
                            |> withQueryStringItem { name = "to"; value = number}
                            |> withQueryStringItem { name = "message"; value = message}
                            |> getResponseAsync 
            return 
                match result.StatusCode with
                    | 201 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask
   
    member this.GetStatus (id) =
        let url = sprintf "%s/%s" baseurl id
        let service =  async {
            let! validAuthToken = validateToken this.identity
            let! result = createRequest HttpMethod.Get url |> withHeader (Authorization validAuthToken.rawToken)  |> getResponseAsync 
            return 
                match result.StatusCode with
                    | 200 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask
