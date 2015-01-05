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

type MailSender (identity:Identity) = 
    let baseurl =(getJsonStringValue (identity.config) "email" ).Value
    member this.identity = identity

    member this.Send (mail) =
        let body = JSONSerializer.toString (mail)
        let service =  async {
            let! result = createRequest HttpMethod.Post baseurl  
                            |> withHeader (Authorization this.identity.rawToken) 
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
   
