module Crash

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open KzApplication
open System.Runtime.InteropServices
open Newtonsoft
open Newtonsoft.Json

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

type CrashInformation = {
    Platform : string
    MachineName : string
    SystemName : string
    OSVersion : string
    FileName : string
    LineNumber : int
    MethodName : string
    ClassName : string
    StackTrace : string
    Reason : string
    AppVersionCode : string
    AppVersionName : string
    BreadCrumbs : array<string>
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
        //System.Diagnostics.Debug.WriteLine("Posting with token:" + identity.rawToken)
        let url = (getJsonStringValue (identity.config) "url" ).Value + "api/v3/logging/crash/xamarin/dump"
        let details =  request.CrashDetails.Replace("\n",String.Empty)
        //System.Diagnostics.Debug.WriteLine("Posting to:" + url)
        return createRequest HttpMethod.Post url 
                |> withHeader (Authorization identity.rawToken)
                |> withHeader (ContentType "application/json")  
                |> withBody details
                |> getResponse
    }

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

    static member CreateCrashMessage (platform , currentDeviceName , currentDeviceSystemName , currentDeviceSystemVersion , filename , linenumber , methodname , classname , fullstack, reason, appVersionCode, appVersionName, breadcrumbs) = 
        let crash = { Platform = platform; MachineName = currentDeviceName; SystemName = currentDeviceSystemName; OSVersion = currentDeviceSystemVersion; FileName=filename; LineNumber = linenumber; MethodName = methodname; ClassName = classname; StackTrace = fullstack; Reason = reason; AppVersionCode = appVersionCode; AppVersionName = appVersionName; BreadCrumbs = breadcrumbs}
        JsonConvert.SerializeObject crash
        