module DataSources

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open KzApplication
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
        return result |> getResponse
    }