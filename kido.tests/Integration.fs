module Integration

open NUnit.Framework
open Protocol
open KidoZenCloudClient
open System.Web
open System.Web.Http 
open System.Net
open KidoZenTypesParser
open System.Collections.Generic
open TestValues

let [<Test>] ``should parse configuration as Application`` () = 
    ServicePointManager.ServerCertificateValidationCallback  <- (fun _ _ _ _  -> true);
    let initializeApplication = initialize getApplicationConfiguration 
    //
    let settings = initializeApplication "armonia" "tasks" |> Async.RunSynchronously
    match settings with
    | Application app ->
        Assert.AreEqual("http://tasks.armonia.kidocloud.com/", app.Scope)         
        Assert.AreEqual("https://armonia.kidocloud.com/auth/v1/WRAPv0.9", app.AuthEndpoint)
        Assert.AreEqual("https://kido-armonia.accesscontrol.windows.net/", app.ServiceScope)
    | _ -> Assert.Fail()

//TODO: por el momento solo funciona si coincide el marketplace con el nombre del tenant y esta en '.kidocloud.com'  
let [<Test>] ``should get token for MarketPlace`` () = 
    let credentials =  { name = "armonia@kidozen.com"; secret = "pass"}
    let tokenRequest = createTokenRequest  "armonia" credentials
    ServicePointManager.ServerCertificateValidationCallback  <- (fun _ _ _ _  -> true);
    let authenticateToMarketplace = authenticate getKidoZenToken
    // 
    let token = authenticateToMarketplace tokenRequest |> Async.RunSynchronously
    match token with
    | Success t -> Assert.NotNull(t)
    | _ -> Assert.Fail()

let [<Test>] ``should get token for Application `` () = 
    let credentials =  { name = "tellago@kidozen.com"; secret = "pass"}
    let tokenRequest = createTokenRequest  "https://tests.qa.kidozen.com" credentials |> forApplication "tasks"
    ServicePointManager.ServerCertificateValidationCallback  <- (fun _ _ _ _  -> true);
    let authenticateToMarketplace = authenticate getKidoZenToken
    // 
    let token = authenticateToMarketplace tokenRequest |> Async.RunSynchronously
    match token with
    | Success t -> Assert.NotNull(t)
    | _ -> Assert.Fail()

let [<Test>] ``should get token for Application using provider`` () = 
    let credentials =  { name = "tellago@kidozen.com"; secret = "pass"}
    let tokenRequest = createTokenRequest  "https://tests.qa.kidozen.com" credentials |> forApplication "tasks" |> usingProvider "Kidozen"
    ServicePointManager.ServerCertificateValidationCallback  <- (fun _ _ _ _  -> true);
    let authenticateToMarketplace = authenticate getKidoZenToken
    // 
    let token = authenticateToMarketplace tokenRequest |> Async.RunSynchronously
    match token with
    | Success t -> Assert.NotNull(t)
    | _ -> Assert.Fail()

let [<Test>] ``should get token for Application in kidocloud`` () = 
    let credentials =  { name = "armonia@kidozen.com"; secret = "pass"}
    let tokenRequest = createTokenRequest  "armonia" credentials |> forApplication "tasks"
    ServicePointManager.ServerCertificateValidationCallback  <- (fun _ _ _ _  -> true);
    let authenticateToMarketplace = authenticate getKidoZenToken
    // 
    let token = authenticateToMarketplace tokenRequest |> Async.RunSynchronously
    match token with
    | Success t -> Assert.NotNull(t)
    | _ -> Assert.Fail()

let [<Test>] `` should invoke service with user and application`` () =
    let credentials =  { name = "tests@kidozen.com"; secret = "pass"}
    let tokenRequest = createTokenRequest "https://tests.qa.kidozen.com" credentials |> forApplication "tasks"
    ServicePointManager.ServerCertificateValidationCallback  <- (fun _ _ _ _  -> true);
    
    let initializeApplication = initialize getApplicationConfiguration 
    let settings = initializeApplication "https://tests.qa.kidozen.com" "tasks" |> Async.RunSynchronously
    match settings with
    | Application a ->
        let authenticateToApplication = authenticate getKidoZenToken
        // 
        let token = authenticateToApplication tokenRequest |> Async.RunSynchronously
        match token with
        | Success t -> 
            let user = t |> parseUserToken
            match user with
            | KidoUser u ->
                let apiRequest = createEapiRequest a u "Google" "get" |> withJsonBody "{\"path\":\"?k=kidozen\"}"
                let invokeGoogleService =  invokeEApi callService 
                let apiResponse = invokeGoogleService apiRequest |> Async.RunSynchronously
                match apiResponse with
                | EApiResponse r ->
                    Assert.AreEqual(200,r.Status);
                | ServiceFail f-> Assert.Fail()
            | Fail f-> Assert.Fail()
            | _ -> Assert.Fail()
        | _ -> Assert.Fail()
    | _ -> Assert.Fail()
    
    
//Para la llamada a los servicios, en el tokenRequest deberia incluirse el parametro "url q viene en la aplicacion"