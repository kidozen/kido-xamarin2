﻿namespace Kidozen

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


[<NoEquality;NoComparison>]
type SubscriptionDetails = {
    mutable applicationName : string;
    mutable channelName : string;
    mutable subscriptionId : string;
    }

type Notifications (appName, channel, tokenOrSubscriptionId, identity:Identity) = 
    let name = appName
    let channel = channel
    let deviceTokenOrSubscriptionId = tokenOrSubscriptionId
    //let mutable subscriptionId = String.Empty // for Android only
    let baseurl =(getJsonStringValue (identity.config) "notification" ).Value
    member this.identity = identity

    member this.Push<'a>(parameters) =
        let url = sprintf "%s/push/%s/%s" baseurl name channel
        let message = JSONSerializer.toString parameters
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") 
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withBody message
                            |> getResponseAsync                             
            return 
                match result.StatusCode with
                    | 200 | 201 | 204 -> true
                    | _ -> raise ( new Exception (result.EntityBody.Value))           
            }
        service |> Async.StartAsTask

    member this.GetSubscriptions() =
        let processResponse value = 
            let token = JObject.Parse(value)    
            if (((token = null) = false) || (token.Type = JTokenType.Array && token.HasValues)) then JsonConvert.DeserializeObject<IEnumerable<SubscriptionDetails>> (value) else null       
                   
        let url = sprintf "%s/devices/%s/%s" baseurl deviceTokenOrSubscriptionId name 
        let service =  async {
            let! result = createRequest HttpMethod.Get url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseAsync
                                            
            return 
                match result.StatusCode with
                    | 200 | 201 -> 
                        match result.EntityBody with
                            | Some v -> processResponse v
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
                    | 200 | 201 | 204-> true
                    | _ -> raise ( new Exception (result.EntityBody.Value))                
            }
        service |> Async.StartAsTask

    member this.Subscribe(parameters) =
        let message = JSONSerializer.toString parameters
        System.Diagnostics.Debug.WriteLine("Push -> Post message: {0}" , message)
        let url = sprintf "%s/subscriptions/%s/%s" baseurl name channel
        System.Diagnostics.Debug.WriteLine("Push -> to url: {0}" , url)        
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") 
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withBody message
                            |> getResponseAsync        
            System.Diagnostics.Debug.WriteLine("Push -> Result Status: {0}" , result.StatusCode)                                                 
            System.Diagnostics.Debug.WriteLine("Push -> Result Body: {0}" , result.EntityBody.Value)
            return 
                match result.StatusCode with
                    | 200 | 201 -> true
                    | _ -> raise ( new Exception (result.EntityBody.Value))                
            }
        service |> Async.StartAsTask
    