module Configuration

open System
open HttpClient
open Utilities
open identityproviders
open KzApplication
open System.Runtime.InteropServices

type ConfigurationEntityOperationType = GetConfiguration  | CreateConfiguration  | DeleteConfiguration

type ConfigurationParametersType = 
    | ConfigurationEntityParam of string          
    | ConfigurationQueryParams of NameValue list  

type ConfigurationRequest = {
        Name : string
        Identity : Identity
        Operation : ConfigurationEntityOperationType option
        Parameters : ConfigurationParametersType option
    }

let createConfigurationRequest name identity =  {
        Name = name;
        Identity = identity;
        Operation = None;
        Parameters = None
    }

let withParameters parameters configRequest = {
    configRequest with Parameters = Some (parameters)  
    }

let withConfigurationOperation operation configRequest = {
    configRequest with Operation = Some (operation)
}


let getResult request = 
    async {
        let! validIdentity = validateToken request.Identity
        let url =(getJsonStringValue (validIdentity.config) "config" ).Value + "/" + request.Name 

        let requestType = 
            match request.Operation with
            | None | Some GetConfiguration -> HttpMethod.Get
            | Some DeleteConfiguration -> HttpMethod.Delete
            | Some CreateConfiguration -> HttpMethod.Post
        
        let dsrequest = createRequest requestType url |> withHeader (Authorization validIdentity.rawToken)

        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | ConfigurationEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | ConfigurationQueryParams l -> 
                        let qslist = Some l
                        dsrequest |> withQueryStringItems qslist 
                | None -> createRequest requestType url |> withHeader (Authorization validIdentity.rawToken)
        return result  |> getResponse
    }
