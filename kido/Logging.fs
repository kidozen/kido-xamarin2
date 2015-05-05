module Logging

open System
open HttpClient
open Utilities
open identityproviders
open KzApplication
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
        let! validAuthToken = validateToken request.Identity
        let baseurl =(getJsonStringValue (validAuthToken.config) "logging-v3" ).Value 

        let requestType = 
            match request.Operation with
            | None | Some GetLogEntity | Some QueryLog -> HttpMethod.Get
            | Some DropLog | Some DeleteLogEntity -> HttpMethod.Delete
            | Some WriteLog -> HttpMethod.Post
        
        let dsrequest = 
            match request.Level with
                    | Some level ->  
                        match request.Message with
                        | Some message -> createRequest requestType baseurl |> withHeader (Authorization validAuthToken.rawToken) |> withQueryStringItem {name="level"; value=level.ToString()} |> withQueryStringItem {name="message"; value=message}
                        | None ->  createRequest requestType baseurl |> withHeader (Authorization validAuthToken.rawToken) |> withQueryStringItem {name="level"; value=level.ToString()}
                    | None -> createRequest requestType baseurl |> withHeader (Authorization validAuthToken.rawToken)
        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | LogEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | LogQueryParams l -> 
                        let qslist = Some l
                        dsrequest |> withQueryStringItems qslist 
                | None -> createRequest requestType baseurl |> withHeader (Authorization validAuthToken.rawToken)
        return result  |> getResponse
    }