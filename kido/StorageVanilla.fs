namespace Kidozen

//Vanilla support
open Storage
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

type ObjectSet (name, identity:Identity) = 
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
