module Configuration

open System
open HttpClient
open Utilities
open identityproviders
open Application
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
        let url =(getJsonStringValue (request.Identity.config) "config" ).Value + "/" + request.Name 

        let requestType = 
            match request.Operation with
            | None | Some GetConfiguration -> HttpMethod.Get
            | Some DeleteConfiguration -> HttpMethod.Delete
            | Some CreateConfiguration -> HttpMethod.Post
        
        let dsrequest = createRequest requestType url |> withHeader (Authorization request.Identity.rawToken)

        let result 
            = match request.Parameters with
                | Some p -> 
                    match p with
                    | ConfigurationEntityParam s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                    | ConfigurationQueryParams l -> 
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

type Configuration (name, identity:Identity) = 
    member this.name:string = name
    member this.identity = identity

    //
    member this.Save (parameters) =
        let entity = ConfigurationEntityParam (JSONSerializer.toString (parameters))
        let create = async {
                let! result = createConfigurationRequest this.name this.identity |> withConfigurationOperation CreateConfiguration |> withParameters entity |> getResult
                return 
                    match result.StatusCode with
                        | 201 -> true
                        | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask

    //
    member this.Delete() = 
        let delete = async {
            let! result = createConfigurationRequest this.name this.identity |>  withConfigurationOperation DeleteConfiguration|> getResult
            return
                match result.StatusCode with
                    | 200 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        delete |> Async.StartAsTask
    
    /// Returns an instance of T
    member this.Get<'a>() =
        let get = async {
            let! result = createConfigurationRequest this.name this.identity |>  withConfigurationOperation GetConfiguration|> getResult
            return match result.StatusCode with
                    | 200 -> 
                        match result.ContentLength with
                            |  l when l > int64(0) -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                            |  _ -> raise (new ArgumentException("Invalid configuration name",this.name))
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask
    
    /// Returns an object as JSON
    member this.Get() =
        let get = async {
            let! result = createConfigurationRequest this.name this.identity |> withConfigurationOperation GetConfiguration|> getResult
            return match result.StatusCode with
                    | 200 -> 
                        match result.ContentLength with
                            |  l when l > int64(0) -> result.EntityBody.Value
                            |  _ -> raise (new ArgumentException("Invalid configuration name",this.name))
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask
