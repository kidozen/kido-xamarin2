namespace Kidozen

//Vanilla support
open Queue
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

type Queue (name, identity:Identity) = 
    member this.name = name
    member this.identity = identity

    //
    member this.Enqueue (parameters) =
        let entity = QueueEntityParam (JSONSerializer.toString (parameters))
        let create = async {
                let! result = createQueueRequest this.name this.identity |> withQueueOperation EnqueueEntity |> withParameters entity |> getResult
                return 
                    match result.StatusCode with
                        | 201 -> true
                        | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask
    
    /// Returns an instance of T
    member this.Dequeue<'a>() =
        let get = async {
            let! result = createQueueRequest this.name this.identity |> withQueueOperation DequeueEntity|> getResult
            return match result.StatusCode with
                    | 200 -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask
    
    /// Returns an object as JSON
    member this.Dequeue() =
        let get = async {
            let! result = createQueueRequest this.name this.identity |> withQueueOperation DequeueEntity|> getResult
            return match result.StatusCode with
                    | 200 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask
