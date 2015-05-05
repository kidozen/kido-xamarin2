module Storage

open System
open HttpClient
open Utilities
open identityproviders
open KzApplication
open System.Runtime.InteropServices

type StorageEntityOperationType = GetStorageEntity | QueryStorage | DropStorage | CreateStorageEntity | UpdateStorageEntity | DeleteStorageEntity

type StorageParametersType = 
    | StorageEntityParam of string          // create, update
    | StorageQueryParams of NameValue list  // query

type StorageRequest = {
        Name : string
        Identity : Identity
        EntityId : string option
        Operation : StorageEntityOperationType option
        Parameters : StorageParametersType option
    }

let createStorageRequest name identity =  {
        Name = name;
        Identity = identity;
        EntityId = None;
        Operation = None;
        Parameters = None
    }

let forEntityWithId id storageRequest = {
    storageRequest with EntityId = Some (id)
    }

let withParameters parameters storageRequest = {
    storageRequest with Parameters = Some (parameters)  
    }

let withStorageOperation operation storageRequest = {
    storageRequest with Operation = Some (operation)
}


let getResult request = 
    async {
        let! validIdentity = validateToken request.Identity
        let baseurl =(getJsonStringValue (validIdentity.config) "storage" ).Value + "/" + request.Name 
        let url = 
            match request.EntityId with
            | Some id ->  sprintf "%s/%s" baseurl id
            | None -> baseurl
    
        let requestType = 
            match request.Operation with
            | None | Some GetStorageEntity | Some QueryStorage -> HttpMethod.Get
            | Some DropStorage | Some DeleteStorageEntity -> HttpMethod.Delete
            | Some CreateStorageEntity -> HttpMethod.Post
            | Some UpdateStorageEntity -> HttpMethod.Put
        
        let dsrequest = createRequest requestType url |> withHeader (Authorization validIdentity.rawToken)

        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | StorageEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | StorageQueryParams l -> 
                        let qslist = Some l
                        dsrequest |> withQueryStringItems qslist 
                | None -> createRequest requestType url |> withHeader (Authorization validIdentity.rawToken)
        return result  |> getResponse
    }