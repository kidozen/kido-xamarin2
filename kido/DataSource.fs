module DataSource

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open Application
open System.Runtime.InteropServices

// DataSources
type DSType = DSGet | DSInvoke

type DSQueryParametersType = 
    | DSGetParams of NameValue list
    | DSInvokeParams of string

type DSRequest = {
        Name : string
        Identity : Identity
        Type : DSType option
        Parameters : DSQueryParametersType option
    }

let createDs name identity =  {
        Name = name;
        Identity = identity;
        Type = None;
        Parameters = None
    }

let withParameters parameters dsRequest = {
    dsRequest with Parameters = Some (parameters)  
    }

let withDSType dstype dsRequest = {
    dsRequest with Type = Some (dstype)
}

let getResult request = 
    async {
        let! identity = validateToken request.Identity
        let url = (getJsonStringValue (identity.config) "datasource" ).Value + "/" + request.Name 
        let requestType = 
            match request.Type with
            | None | Some DSGet -> HttpMethod.Get
            | Some DSInvoke -> HttpMethod.Post
        let dsrequest = createRequest requestType url |> withHeader (Authorization identity.rawToken)
        
        let result 
            = match request.Parameters with
                    | Some p -> 
                        match p with
                        | DSInvokeParams s -> dsrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                        | DSGetParams l -> 
                            let qslist = Some l
                            dsrequest |> withQueryStringItems qslist 
                    | None -> dsrequest

        return result  |> getResponse
    }

open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic

type DataSource (name, identity:Identity) = 
    member this.dsname = name
    member this.identity = identity

    member this.Invoke () = 
        let invoke = async {
                let! result = createDs this.dsname this.identity |> withDSType DSInvoke|> getResult
                return match result.StatusCode with
                       | 200 -> result.EntityBody.Value
                       | _ -> raise ( new Exception (result.EntityBody.Value))
            }
        invoke |> Async.StartAsTask
        
    member this.Query () = 
        let query = async {
                let! result = createDs this.dsname this.identity |> getResult
                return match result.StatusCode with
                       | 200 -> result.EntityBody.Value
                       | _ -> raise ( new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask

    member this.Invoke (parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let dsParams = DSInvokeParams paramsAsString 
        let invoke = async {
                let! result = createDs this.dsname this.identity |> withDSType DSInvoke |> withParameters dsParams |> getResult
                return match result.StatusCode with
                       | 200 -> result.EntityBody.Value
                       | _ -> raise ( new Exception (result.EntityBody.Value))
            }
        invoke |> Async.StartAsTask
   
    member this.Query (parameters) =
        let paramsAsValueList = JSONSerializer.toNameValueList parameters
        let dsParams = DSGetParams paramsAsValueList 
        let query = async {
                let! result = createDs this.dsname this.identity |> withParameters dsParams |> getResult
                return match result.StatusCode with
                       | 200 -> result.EntityBody.Value
                       | _ -> raise ( new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask

    // TODO: Refactor to reuse
    member this.Invoke<'a>() = 
            let invoke = async {
                    let! result = createDs this.dsname this.identity |> withDSType DSInvoke|> getResult
                    return match result.StatusCode with
                           | 200 -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                           | _ -> raise ( new Exception (result.EntityBody.Value))
                }
            invoke |> Async.StartAsTask
        
    member this.Query<'a>() = 
        let query = async {
                let! result = createDs this.dsname this.identity |> getResult
                return match result.StatusCode with
                       | 200 -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                       | _ -> raise ( new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask

    member this.Invoke<'a>(parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let dsParams = DSInvokeParams paramsAsString 
        let invoke = async {
                let! result = createDs this.dsname this.identity |> withDSType DSInvoke |> withParameters dsParams |> getResult
                return match result.StatusCode with
                       | 200 -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                       | _ -> raise ( new Exception (result.EntityBody.Value))
            }
        invoke |> Async.StartAsTask
   
    member this.Query<'a>(parameters) =
        let paramsAsValueList = JSONSerializer.toNameValueList parameters
        let dsParams = DSGetParams paramsAsValueList 
        let query = async {
                let! result = createDs this.dsname this.identity |> withParameters dsParams |> getResult
                return match result.StatusCode with
                       | 200 -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                       | _ -> raise ( new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask