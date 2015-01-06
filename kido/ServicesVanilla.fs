namespace Kidozen

open Services
open KzApplication
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

type Service (name, identity:Identity) = 
    member this.dsname = name
    member this.identity = identity

    member this.Invoke (methodName:string, parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let dsParams = SvcInvokeParams paramsAsString 
        let service =  async {
            let! result = createService this.dsname methodName this.identity |> withSvcType SvcInvoke |> withParameters dsParams |> getResult 
            return 
                match result.StatusCode with
                    | 200 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask

    //File support
    member private this.processFileResponse result = 
        if (Map.containsKey ResponseHeader.ContentDisposition result.Headers) then
            result.BytesBody
        else
            raise ( new Exception ("Content-Disposition header was not found"))
   
    member this.InvokeFile (methodName:string, parameters) =
        let paramsAsString = JSONSerializer.toString parameters
        let service =  async {
                let url = (getJsonStringValue (identity.config) "service" ).Value + "/" + this.dsname + "/invoke/" + methodName
                let! result = createRequest HttpMethod.Post url  
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
        service |> Async.StartAsTask
  