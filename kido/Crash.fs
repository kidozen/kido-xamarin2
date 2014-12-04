module Crash

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open Application
open System.Runtime.InteropServices

open Kidozen

let getKeyToken marketplace application key = async {
    let! result = asyncGetKeyToken marketplace application key
    match result with
        | Token t -> return t
        | InvalidApplication e -> return raise e
        | InvalidCredentials e -> return raise e
        | InvalidIpCredentials e -> return raise e
}

// Crash
type CrashRequest = {
        Marketplace : string
        Application : string
        AppKey : string
        CrashDetails : string
    }

let createCrashRequest crash marketplace application key =  {
        Marketplace = marketplace
        Application = application
        AppKey = key
        CrashDetails = crash
    }

let getResult request = 
    async {
        let! identity = getKeyToken request.Marketplace request.Application request.AppKey
        System.Diagnostics.Debug.WriteLine("Posting with token:" + identity.rawToken)
        let url = (getJsonStringValue (identity.config) "url" ).Value + "api/v3/logging/crash/xamarin/dump"
        System.Diagnostics.Debug.WriteLine("Posting to:" + url)
        return createRequest HttpMethod.Post url 
                |> withHeader (Authorization identity.rawToken)
                |> withHeader (ContentType "application/json")  
                |> withBody request.CrashDetails
                |> getResponse
    }

let crashInformation = 
    sprintf "{\"Platform\":\"%s\",\"MachineName\":\"%s\",\"SystemName\":\"%s\",\"OSVersion\":\"%s\",\"FileName\":\"%s\",\"LineNumber\":\"%d\",\"MethodName\":\"%s\",\"ClassName\":\"%s\",\"StackTrace\":\"%s\",\"Reason\":\"%s\",\"AppVersionCode\":\"%s\",\"AppVersionName\":\"%s\"}"

// Vanilla support
type Crash () = 
    static member Create (crash, marketplace, application, key) =
        let create = async {
            let! result = createCrashRequest  crash marketplace application key  |> getResult
            System.Diagnostics.Debug.WriteLine("Crash post Status Code Response:" + result.StatusCode.ToString())
            return 
                match result.StatusCode with
                    | 201 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask

    static member CreateCrashMessage (platform , currentDeviceName , currentDeviceSystemName , currentDeviceSystemVersion , filename , linenumber , methodname , classname , fullstack, reason, appVersionCode, appVersionName) = 
        crashInformation platform 
                        currentDeviceName 
                        currentDeviceSystemName 
                        currentDeviceSystemVersion 
                        filename 
                        linenumber 
                        methodname 
                        classname 
                        fullstack
                        reason
                        appVersionCode
                        appVersionName