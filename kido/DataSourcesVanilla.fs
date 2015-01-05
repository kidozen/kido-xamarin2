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
                | 200 ->JsonConvert.DeserializeObject<'a>(content)
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
        //| 200 -> result.EntityBody.Value
    member this.Invoke () = 
        this.Invoke<string>()
        
    member this.Query () = 
        this.Query<string>()

    member this.Invoke (parameters) =
        this.Invoke<string>(parameters)
   
    member this.Query (parameters) =
        this.Query<string>(parameters)

    //File support
    //if(jqXHR.getResponseHeader('Content-Disposition') === 'attachment') {
    member this.InvokeFile() = 
            let invoke = async {
                let! result = createDs this.dsname this.identity |> withDSType DSInvoke|> getResult
                return this.ProcessResponse<byte[]>(result)
            }
            invoke |> Async.StartAsTask

    member this.QueryFile() = 
        let query = async {
            let! result = createDs this.dsname this.identity |> getResult
            return this.ProcessResponse<byte[]>(result)
        }
        query |> Async.StartAsTask

    member this.InvokeFile(parameters) = 
        let paramsAsString = JSONSerializer.toString parameters
        let dsParams = DSInvokeParams paramsAsString 
        let invoke = async {
            let! result = createDs this.dsname this.identity |> withDSType DSInvoke |> withParameters dsParams |> getResult
            return this.ProcessResponse<byte[]>(result)
        }
        invoke |> Async.StartAsTask

    member this.QueryFile(parameters) = 
        let paramsAsValueList = JSONSerializer.toNameValueList parameters
        let dsParams = DSGetParams paramsAsValueList 
        let query = async {
            let! result = createDs this.dsname this.identity |> withParameters dsParams |> getResult
            return this.ProcessResponse<byte[]>(result)
        }
        query |> Async.StartAsTask

