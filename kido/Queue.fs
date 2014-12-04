module Queue

open System
open HttpClient
open Utilities
open identityproviders
open Application
open System.Runtime.InteropServices

type QueueOperationType = EnqueueEntity | DequeueEntity

type QueueParametersType = 
    | QueueEntityParam of string          // create, update
    | QueueQueryListParams of NameValue list  // query

type QueueRequest = {
        Name : string
        Identity : Identity
        Operation : QueueOperationType option
        Parameters : QueueParametersType option
    }

let createQueueRequest name identity =  {
        Name = name;
        Identity = identity;
        Operation = None;
        Parameters = None
    }

let withParameters parameters queueRequest = {
    queueRequest with Parameters = Some (parameters)  
    }

let withQueueOperation operation queueRequest = {
    queueRequest with Operation = Some (operation)
}


let getResult request = 
    async {
        let baseurl =(getJsonStringValue (request.Identity.config) "queue" ).Value + "/" + request.Name 
        let url = 
            match request.Operation with
            | Some DequeueEntity ->  sprintf "%s/%s" baseurl "next"
            | _ -> baseurl
    
        let requestType = 
            match request.Operation with
            | Some DequeueEntity -> HttpMethod.Delete
            | None | Some EnqueueEntity -> HttpMethod.Post
        
        let dsrequest = createRequest requestType url |> withHeader (Authorization request.Identity.rawToken)

        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | QueueEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | QueueQueryListParams l -> 
                        let qslist = Some l
                        dsrequest |> withQueryStringItems qslist 
                | None -> createRequest requestType url |> withHeader (Authorization request.Identity.rawToken)
        return result  |> getResponse
    }

//Vanilla support
open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic
open Serialization

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

