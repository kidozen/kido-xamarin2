module Storage

open System
open HttpClient
open Utilities
open identityproviders
open Application
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
        let baseurl =(getJsonStringValue (request.Identity.config) "storage" ).Value + "/" + request.Name 
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
        
        let dsrequest = createRequest requestType url |> withHeader (Authorization request.Identity.rawToken)

        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | StorageEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | StorageQueryParams l -> 
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

[<NoEquality;NoComparison>]
type Metadata = {
    sync: int;
    isPrivate: bool;
    updatedOn: DateTime;
    updatedBy: string;
    createdOn: DateTime;
    createdBy: string;
    }

[<NoEquality;NoComparison>]
type EntityMetadata = {
    _id: string;
    _metadata: Metadata
}

type Storage (name, identity:Identity) = 
    member this.name = name
    member this.identity = identity

    //
    member this.Create (parameters) =
        let entity = StorageEntityParam (JSONSerializer.toString (parameters))
        let create = async {
                let! result = createStorageRequest this.name this.identity |> withStorageOperation CreateStorageEntity |> withParameters entity |> getResult
                return 
                    match result.StatusCode with
                        | 201 -> JsonConvert.DeserializeObject<EntityMetadata>(result.EntityBody.Value)
                        | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask
    
    //
    member this.Update (id, parameters) =
        let entity = StorageEntityParam (JSONSerializer.toString (parameters))
        let path = sprintf "%s/%s" this.name id
        let create = async {
                let! result = createStorageRequest path this.identity |> withStorageOperation UpdateStorageEntity |> withParameters entity |> getResult
                return 
                    match result.StatusCode with
                        | 200 -> JsonConvert.DeserializeObject<EntityMetadata>(result.EntityBody.Value)
                        | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask
    
    //
    member this.Save (parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let id = getJsonStringValue "_id" paramsAsString
        let entity = StorageEntityParam paramsAsString

        let path = match id with 
                   | Some _id -> sprintf "%s/%s" this.name _id
                   | None -> this.name

        let storageOperation = match id with
                                | Some _id -> UpdateStorageEntity
                                | None -> CreateStorageEntity

        let create = async {
                let! result = createStorageRequest path this.identity |> withStorageOperation storageOperation |> withParameters entity |> getResult
                return 
                    match result.StatusCode with
                        | 200 | 201 -> JsonConvert.DeserializeObject<EntityMetadata>(result.EntityBody.Value)
                        | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask

    //
    member this.Delete (id) = 
        let delete = async {
            let! result = createStorageRequest this.name this.identity |> forEntityWithId id |> withStorageOperation DeleteStorageEntity|> getResult
            return
                match result.StatusCode with
                    | 200 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        delete |> Async.StartAsTask
    //
    member this.Clear () = 
        let delete = async {
            let! result = createStorageRequest this.name this.identity |> withStorageOperation DeleteStorageEntity|> getResult
            return
                match result.StatusCode with
                    | 200 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        delete |> Async.StartAsTask

    /// Returns an instance of T
    member this.Get<'a>(id) =
        let get = async {
            let! result = createStorageRequest this.name this.identity |> forEntityWithId id |> withStorageOperation GetStorageEntity|> getResult
            return match result.StatusCode with
                    | 200 -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask
    
    /// Returns an object as JSON
    member this.Get(id) =
        let get = async {
            let! result = createStorageRequest this.name this.identity |> forEntityWithId id |> withStorageOperation GetStorageEntity|> getResult
            return match result.StatusCode with
                    | 200 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask

    /// Query and returns an string representation
    member this.Query (query) =
        let queryParams = StorageQueryParams [{name="query"; value=query}] 
        let query = async {
            let! result = createStorageRequest this.name this.identity |> withStorageOperation QueryStorage |> withParameters queryParams  |> getResult 
            return match result.StatusCode with
                    | 200 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask

    /// Query and returns an IEnumerable<T>
    member this.Query<'a>(query) =
        let queryParams = StorageQueryParams [{name="query"; value=query}] 
        let query = async {
            let! result = createStorageRequest this.name this.identity |> withStorageOperation QueryStorage |> withParameters queryParams  |> getResult 
            return match result.StatusCode with
                    | 200 -> JsonConvert.DeserializeObject<IEnumerable<'a>>(result.EntityBody.Value)
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask
