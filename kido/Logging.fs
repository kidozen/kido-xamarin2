module Logging

open System
open HttpClient
open Utilities
open identityproviders
open Application
open System.Runtime.InteropServices

type LogEntityOperationType = GetLogEntity | QueryLog | DropLog | WriteLog | DeleteLogEntity

type LogParametersType = 
    | LogEntityParam of string          // create, update
    | LogQueryParams of NameValue list  // query

type LogRequest = {
        Identity : Identity 
        Message : String option
        Level : int option
        Parameters : LogParametersType option
        Operation : LogEntityOperationType option
    }

let createLogRequest identity =  {
        Identity = identity;        
        Message = None;
        Level = None;
        Operation = None;
        Parameters = None
    }

let withMessage message logRequest = {
    logRequest with Message = Some (message)  
    }

let withLevel level logRequest = {
    logRequest with Level = Some (level)  
    }

let withParameters parameters logRequest = {
    logRequest with Parameters = Some (parameters)  
    }

let withLogOperation operation logRequest = {
    logRequest with Operation = Some (operation)
}


let getResult request = 
    async {
        let baseurl =(getJsonStringValue (request.Identity.config) "logging-v3" ).Value 

        let requestType = 
            match request.Operation with
            | None | Some GetLogEntity | Some QueryLog -> HttpMethod.Get
            | Some DropLog | Some DeleteLogEntity -> HttpMethod.Delete
            | Some WriteLog -> HttpMethod.Post
        
        let dsrequest = 
            match request.Level with
                    | Some level ->  
                        match request.Message with
                        | Some message -> createRequest requestType baseurl |> withHeader (Authorization request.Identity.rawToken) |> withQueryStringItem {name="level"; value=level.ToString()} |> withQueryStringItem {name="message"; value=message}
                        | None ->  createRequest requestType baseurl |> withHeader (Authorization request.Identity.rawToken) |> withQueryStringItem {name="level"; value=level.ToString()}
                    | None -> createRequest requestType baseurl |> withHeader (Authorization request.Identity.rawToken)

        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | LogEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | LogQueryParams l -> 
                        let qslist = Some l
                        dsrequest |> withQueryStringItems qslist 
                | None -> createRequest requestType baseurl |> withHeader (Authorization request.Identity.rawToken)
        return result  |> getResponse
    }

//Vanilla support
open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic
open Serialization

type LogLevel = 
    Verbose = 0 
    | Information = 1 
    | Warning = 2 
    | Error =3 
    | Critical = 4

type Log (identity:Identity) = 
    let mutable logidentity = identity

    let getKeyToken marketplace application key = async {
        let! result = asyncGetKeyToken marketplace application key
        match result with
            | Token t -> return t
            | InvalidApplication e -> return raise e
            | InvalidCredentials e -> return raise e
            | InvalidIpCredentials e -> return raise e
    }

    //
    member this.Create (message, data, level:LogLevel, marketplace, application, key) =
        let lvl = (int)level
        let entity = LogEntityParam (JSONSerializer.toString (data))
        let create = async {
            //this method could be called before you call authenticate
            let! createIdentity = 
                match identity.rawToken with
                    | "" -> getKeyToken marketplace application key
                    | _ -> async {return logidentity}

            let! result = createLogRequest createIdentity  |> withMessage message  |> withLevel lvl |> withLogOperation WriteLog |> withParameters entity |> getResult
            return 
                match result.StatusCode with
                    | 201 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask
    
    member this.Clear () = 
        let delete = async {
            let! result = createLogRequest logidentity |> withLogOperation DropLog |> getResult
            return
                match result.StatusCode with
                    | 204 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        delete |> Async.StartAsTask

    /// Query and returns an string representation
    member this.Query (query) =
        let queryParams = LogQueryParams [{name="query"; value=query}] 
        let query = async {
            let! result = createLogRequest logidentity |> withLogOperation QueryLog |> withParameters queryParams  |> getResult 
            return match result.StatusCode with
                    | 200 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask

    /// Query and returns an IEnumerable<T>
    member this.Query<'a>(query) =
        let queryParams = LogQueryParams [{name="query"; value=query}] 
        let query = async {
            let! result = createLogRequest logidentity |> withLogOperation QueryLog |> withParameters queryParams  |> getResult 
            return match result.StatusCode with
                    | 200 -> JsonConvert.DeserializeObject<IEnumerable<'a>>(result.EntityBody.Value)
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask
            /// Query and returns an string representation

