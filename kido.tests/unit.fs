module UnitTests

open NUnit.Framework
open KidoZenCloudClient
open KidoZenTypesParser
open Protocol
open TestValues

let [<Test>] ``should parse configuration as Application`` () = 
    //mock
    let mockedconfig appcenter appname =
        parseApplicationSettings appconfigasstring
    let initializeWithMock = initialize mockedconfig
    //
    let settings = initializeWithMock "armonia" "tasks" |> Async.RunSynchronously
    match settings with 
    | Application app -> 
        Assert.AreEqual("http://tasks.armonia.kidocloud.com/", app.Scope)         
        Assert.AreEqual("https://armonia.kidocloud.com/auth/v1/WRAPv0.9", app.AuthEndpoint)
        Assert.AreEqual("https://kido-armonia.accesscontrol.windows.net/", app.ServiceScope)
    | _ -> Assert.Fail()

let [<Test>] ``should get token for MarketPlace`` () = 
    //mock
    let credentials =  { name = "christian"; secret = "pass"}
    let tokenRequest = createTokenRequest  "armonia" credentials
    let getmockedtoken tokenRequest =
        wrapv9tokenasstring 
    let authenticateWithMock = authenticate getmockedtoken
    // 
    let token = authenticateWithMock tokenRequest  |> Async.RunSynchronously
    match token with
    |  t -> Assert.NotNull(t)
    | _ -> Assert.Fail()

let [<Test>] ``should get token for Application`` () = 
     //mock
    let credentials =  { name = "christian"; secret = "pass"}
    let tokenRequest = createTokenRequest  "armonia" credentials |> forApplication "tasks"
    let getmockedtoken tokenRequest =
        wrapv9tokenasstring 
    let authenticateWithMock = authenticate getmockedtoken
    // 
    let token = authenticateWithMock tokenRequest
    match token with
    |  t -> Assert.NotNull(t)
    | _ -> Assert.Fail()

let [<Test>] ``should get token for Application using Provider`` () = 
     //mock
    let credentials =  { name = "christian"; secret = "pass"}
    let tokenRequest = createTokenRequest  "armonia" credentials |> forApplication "tasks" |> usingProvider "x"
    let getmockedtoken tokenRequest =
        wrapv9tokenasstring 
    let authenticateWithMock = authenticate getmockedtoken
    // =>
    let token = authenticateWithMock tokenRequest
    match token with
    |  t -> Assert.NotNull(t)
    | _ -> Assert.Fail()

