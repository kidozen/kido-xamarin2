﻿module Services

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open KzApplication
open System.Runtime.InteropServices

type ServiceType = SvcGet | SvcInvoke

type SvcQueryParametersType = 
    | SvcGetParams of NameValue list
    | SvcInvokeParams of string

type SvcRequest = {
        Name : string
        MethodName : string
        Identity : Identity
        Type : ServiceType option
        Parameters : SvcQueryParametersType option
    }

let createService name methodName identity =  {
        Name = name;
        MethodName = methodName;
        Identity = identity;
        Type = None;
        Parameters = None
    }

let withParameters parameters serviceRequest = {
    serviceRequest with Parameters = Some (parameters)  
    }

let withSvcType svctype serviceRequest = {
    serviceRequest with Type = Some (svctype)
}

let getResult request = 
    async {
        let! identity = validateToken request.Identity
        let url = (getJsonStringValue (identity.config) "service" ).Value + "/" + request.Name + "/invoke/" + request.MethodName
        let requestType = 
            match request.Type with
            | None | Some SvcGet -> HttpMethod.Get
            | Some SvcInvoke -> HttpMethod.Post
        let svcrequest = createRequest requestType url |> withHeader (Authorization identity.rawToken)
        
        let result 
            = match request.Parameters with
                    | Some p -> 
                        match p with
                        | SvcInvokeParams s -> svcrequest |>  withHeader (ContentType "application/json")  |> withHeader (Accept "application/json") |> withBody s
                        | SvcGetParams l -> 
                            let qslist = Some l
                            svcrequest |> withQueryStringItems qslist 
                    | None -> svcrequest

        return result  |> getResponse
    }