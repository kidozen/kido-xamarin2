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

type PubSubMessageArrivedDelegate = delegate of obj * EventArgs -> unit

[<AllowNullLiteral>]
type ISubscriber =
    [<CLIEvent>]
    abstract member OnMessageEvent : IEvent<PubSubMessageArrivedDelegate, EventArgs>
    abstract member Subscribe : String -> String -> Boolean

type PubSub (name, identity:Identity) = 
    let name = name
    let subscriber:ISubscriber = null 
    let subscribeUrl =(getJsonStringValue (identity.config) "ws" ).Value
    let publishUrl =(getJsonStringValue (identity.config) "pubsub" ).Value
  
    member this.identity = identity
    member val SubscriberInstance = subscriber with get, set

    member this.Publish(parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let url = sprintf "%s/%s" publishUrl name
        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withHeader (ContentType "application/json")
                            |> withBody paramsAsString
                            |> getResponseAsync                             
            return 
                match result.StatusCode with
                    | 200 | 201  -> true
                    |   _ -> raise ( new Exception (result.EntityBody.Value))                
            }
        service |> Async.StartAsTask
    
    member this.Subscribe() = 
        let url = subscribeUrl
        let channel = name
        let service =  async {
            return subscriber.Subscribe url channel
        }
        service |> Async.StartAsTask
