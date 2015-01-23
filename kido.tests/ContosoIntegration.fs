module ContosoIntegration

open NUnit.Framework
open Protocol
open KidoZenCloudClient
open System.Web
open System.Web.Http 
open System.Net
open KidoZenTypesParser
open System.Collections.Generic
open TestValues

// kidozen , should I added to core?
type UserForApplication = {
    Usr: KidoToken;      
    App: Application
}

type TokenAndEndpoint = {
    Token : string;
    ServiceEndpoint : string
    }
//
// need settings before getting token, thats why async.RunSync....
// need token before call any service, thats why async.RunSync....
//
let getTkEpForMarketplaceUserAndApp user pass mkp app svc = 
    let credentials =  { name = user; secret = pass}
    let tokenRequest = createTokenRequest mkp credentials |> forApplication app
    let settings = initialize getApplicationConfiguration mkp app |> Async.RunSynchronously
    match settings with
    | Application a ->
        let token = authenticate getKidoZenToken tokenRequest |> Async.RunSynchronously
        match token with
            | Success t -> 
                let user = t |> parseUserToken;
                let ep = List.find (fun i -> i.Service.Equals(svc) ) a.Services
                match user with
                    | KidoToken u -> { Token = u.Token ; ServiceEndpoint = ep.Endpoint }
                    | _ -> failwith "invalid user" 
            | _ -> failwith "invalid token"
    | _ -> failwith "invalid settings"

let TokenAndServiceForTaskInContoso =
    getTkEpForMarketplaceUserAndApp "contoso@kidozen.com" "pass" "https://contoso.local.kidozen.com" "tasks" "service"

let [<Test>] ``should invoke service with user and application`` () =
    ServicePointManager.ServerCertificateValidationCallback  <- (fun _ _ _ _  -> true);
    let tokenandep = TokenAndServiceForTaskInContoso
    let apiRequest = createEapiRequest tokenandep.ServiceEndpoint tokenandep.Token "Weather" "get" |> withJsonBody "{\"path\":\"?q=buenos aires,ar\"}"
    let apiResponse = invokeEApi callService apiRequest |> Async.RunSynchronously
    match apiResponse with
    | EApiResponse r ->
        Assert.AreEqual(200,r.Status);
    | ServiceFail f-> Assert.Fail()

