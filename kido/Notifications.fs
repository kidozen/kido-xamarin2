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

type Notifications (appName, channel, tokenOrSubscriptionId, identity:Identity) = 
    let name = appName
    let channel = channel
    let deviceTokenOrSubscriptionId = tokenOrSubscriptionId
    let mutable subscriptionId = String.Empty // for Android only
    let baseurl =(getJsonStringValue (identity.config) "notification" ).Value
    member this.identity = identity

    member this.Push<'a>(parameters) =
        let url = sprintf "%s/subscriptions/%s/%s/%s" baseurl name channel deviceTokenOrSubscriptionId
        let message = JSONSerializer.toString parameters
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withBody message
                            |> getResponseAsync                             
            return 
                match result.StatusCode with
                    | 200 | 201 -> true
                    | _ -> raise ( new Exception (result.EntityBody.Value))           
            }
        service |> Async.StartAsTask

    member this.GetSubscriptions() =
        let url = sprintf "%s/subscriptions/%s/%s/%s" baseurl name channel deviceTokenOrSubscriptionId
        let service =  async {
            let! result = createRequest HttpMethod.Get url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseAsync                             
            return 
                match result.StatusCode with
                    | 200 | 201 -> 
                        match result.EntityBody with
                            | Some v -> JsonConvert.DeserializeObject<IEnumerable<obj>> (v)
                            | None -> raise( new Exception ("Invalid response") )
                    | _ -> raise ( new Exception (result.EntityBody.Value))                
            }
        service |> Async.StartAsTask

    member this.UnSubscribe() =
        let url = sprintf "%s/subscriptions/%s/%s/%s" baseurl name channel deviceTokenOrSubscriptionId
        let service =  async {
            let! result = createRequest HttpMethod.Delete url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseAsync                             
            return 
                match result.StatusCode with
                    | 200 | 201 -> true
                    | _ -> raise ( new Exception (result.EntityBody.Value))                
            }
        service |> Async.StartAsTask

    member this.Subscribe(parameters) =
        let message = JSONSerializer.toString parameters
        let url = sprintf "%s/subscriptions/%s/%s/%s" baseurl name channel deviceTokenOrSubscriptionId
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withBody message
                            |> getResponseAsync                             
            return 
                match result.StatusCode with
                    | 200 | 201 -> true
                    | _ -> raise ( new Exception (result.EntityBody.Value))                
            }
        service |> Async.StartAsTask
    
