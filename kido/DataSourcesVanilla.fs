namespace Kidozen

open DataSources
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

open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic

type DataSource (name, identity:Identity) = 
    member this.dsname = name
    member this.identity = identity

    member private this.ProcessResponse<'a> result =
        let content = result.EntityBody.Value
        match result.StatusCode with
                | 200 ->
                    let error = getJsonObjectValue content "error" 
                    let data = getJsonObjectValue content "data" 
                    match data with
                        | Some d -> JsonConvert.DeserializeObject<'a>(d)
                        | _ -> match error with
                                    | Some e -> raise ( new Exception (e))
                                    | _ -> raise ( new Exception ("Unknown error."))
                | _ -> raise ( new Exception (result.EntityBody.Value))


    member this.Invoke<'a>() = 
            let invoke = async {
                let! result = createDs this.dsname this.identity |> withDSType DSInvoke|> getResult
                return this.ProcessResponse<'a>(result)
            }
            invoke |> Async.StartAsTask
        
    member this.Query<'a>() = 
        let query = async {
            let! result = createDs this.dsname this.identity |> getResult
            return this.ProcessResponse<'a>(result )
        }
        query |> Async.StartAsTask

    member this.Invoke<'a>(parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let dsParams = DSInvokeParams paramsAsString 
        let invoke = async {
            let! result = createDs this.dsname this.identity |> withDSType DSInvoke |> withParameters dsParams |> getResult
            return this.ProcessResponse<'a>(result)
        }
        invoke |> Async.StartAsTask
   
    member this.Query<'a>(parameters) =
        let paramsAsValueList = JSONSerializer.toNameValueList parameters
        let dsParams = DSGetParams paramsAsValueList 
        let query = async {
            let! result = createDs this.dsname this.identity |> withParameters dsParams |> getResult
            return this.ProcessResponse<'a>(result)
        }
        query |> Async.StartAsTask

    member this.Invoke () = 
        this.Invoke<string>()
        
    member this.Query () = 
        this.Query<string>()

    member this.Invoke (parameters) =
        this.Invoke<string>(parameters)
   
    member this.Query (parameters) =
        this.Query<string>(parameters)

    //File support
    member private this.processFileResponse result = 
        if (Map.containsKey ResponseHeader.ContentDisposition result.Headers) then
            result.BytesBody
        else
            raise ( new Exception ("Content-Disposition header was not found"))

    member this.QueryFile() = 
        let query = async {
            let url = (getJsonStringValue (identity.config) "datasource" ).Value + "/" + this.dsname
            let! result = 
                createRequest HttpMethod.Get url  
                        |> withHeader (Authorization this.identity.rawToken) 
                    |> getResponseBytesAsync 

            return 
                match result.StatusCode with
                    | 200 -> this.processFileResponse result
                    | _ -> raise (new Exception (result.EntityBody.Value))

        }
        query |> Async.StartAsTask

    member this.QueryFile(parameters) = 
        let paramsAsValueList = JSONSerializer.toNameValueList parameters
        let dsParams = Some paramsAsValueList 
        let query = async {
            let url = (getJsonStringValue (identity.config) "datasource" ).Value + "/" + this.dsname
            let! result = 
                createRequest HttpMethod.Get url  
                    |> withHeader (Authorization this.identity.rawToken) 
                    |> withQueryStringItems dsParams
                    |> getResponseBytesAsync 
            return 
                match result.StatusCode with
                    | 200 -> this.processFileResponse result
                    | _ -> raise (new Exception (result.EntityBody.Value))
        }
        query |> Async.StartAsTask

    member this.InvokeFile() = 
            let invoke = async {
                let url = (getJsonStringValue (identity.config) "datasource" ).Value + "/" + this.dsname
                let! result = 
                    createRequest HttpMethod.Post url  
                        |> withHeader (Authorization this.identity.rawToken) 
                        |> getResponseBytesAsync 
            return 
                match result.StatusCode with
                    | 200 -> this.processFileResponse result
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
            invoke |> Async.StartAsTask

    member this.InvokeFile(parameters) = 
        let paramsAsString = JSONSerializer.toString parameters
        let invoke = async {
                let url = (getJsonStringValue (identity.config) "datasource" ).Value + "/" + this.dsname
                let! result = 
                    createRequest HttpMethod.Post url  
                        |> withHeader (Authorization this.identity.rawToken) 
                        |> withHeader (ContentType "application/json")  
                        |> withHeader (Accept "application/json") 
                        |> withBody paramsAsString
                        |> getResponseBytesAsync 
            return 
                match result.StatusCode with
                    | 200 -> this.processFileResponse result
                    | _ -> raise (new Exception (result.EntityBody.Value))
        }
        invoke |> Async.StartAsTask