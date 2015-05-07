module Queue

open System
open HttpClient
open Utilities
open identityproviders
open KzApplication
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
        let! validIdentity = validateToken request.Identity
        let baseurl =(getJsonStringValue (validIdentity.config) "queue" ).Value + "/" + request.Name 
        let url = 
            match request.Operation with
            | Some DequeueEntity ->  sprintf "%s/%s" baseurl "next"
            | _ -> baseurl
    
        let requestType = 
            match request.Operation with
            | Some DequeueEntity -> HttpMethod.Delete
            | None | Some EnqueueEntity -> HttpMethod.Post
        
        let dsrequest = createRequest requestType url |> withHeader (Authorization validIdentity.rawToken)

        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | QueueEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | QueueQueryListParams l -> 
                        let qslist = Some l
                        dsrequest |> withQueryStringItems qslist 
                | None -> createRequest requestType url |> withHeader (Authorization validIdentity.rawToken)
        return result  |> getResponse
    }