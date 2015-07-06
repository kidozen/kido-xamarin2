module DataSyncTests
open System
open System.Threading
open System.Net
open System.Linq
open System.Diagnostics

open NUnit.Framework
open FsUnit

open Nancy
open Nancy.Hosting.Self
open FakeServer

open Kidozen
open Kidozen.DataSync

type myEntity() = 
    inherit DataSyncDocument()
    member val Property1 = "" with get, set

let hostConfig = new HostConfiguration()
hostConfig.AllowChunkedEncoding <- false
// Enabling chunked encoding breaks HEAD requests if you're self-hosting.
// It also seems to mean the Content-Length isn't set in some cases.
hostConfig.UrlReservations<-UrlReservations(CreateAutomatically=true)

let kidozenMarketplace = "http://localhost:1234"
let fakeKidozenHost = new NancyHost(hostConfig, new Uri(kidozenMarketplace))
let fakeIpHost = new NancyHost(hostConfig, new Uri("http://localhost:1235"))

[<TestFixture>]
type ``Given a dummy KidoZen instance`` ()= 
    let kido = new KidoApplication(kidozenMarketplace,"tests","testAppkey")
    let datasync = kido.DataSync<myEntity>("dummykido")

    [<TestFixtureSetUp>]
    member this.fixtureSetup() = 
        datasync |> should not' (be Null)

    [<Test>] member this.
     ``Should create an instance of DataSync`` ()=
      kido.DataSync<string>("abc") |> should not' (be Null)

    [<Test>] 
    member this.``Should create a local document`` ()=
        datasync.Create( myEntity (Property1="a") ) |> should not' (be NullOrEmptyString)
    
    [<Test>] 
    member this.``Should delete a local document`` ()=
        let entity = myEntity(Property1 = "b")
        let id = datasync.Create(entity) 
        id |> should not' (be NullOrEmptyString)
        let qe = datasync.Query(fun r->(r.Property1.Equals("b"))).FirstOrDefault()
        datasync.Delete( qe ) |> should be True

    [<Test>] 
    member this.``Should drop datasync`` ()=
        let ds = kido.DataSync<myEntity>("todrop")
        let entity = myEntity(Property1 = "b")
        let id = ds.Create(entity) 
        id |> should not' (be NullOrEmptyString)        
        ds.Drop()

    //[<Test>] 
    member this.``Should update a local document`` ()=
        let query v = datasync.Query(fun r->(r.Property1.Equals(v))).FirstOrDefault()
        let entity = myEntity(Property1 = "b")
        datasync.Create(entity) |> should not' (be NullOrEmptyString)
        let qe = query "b"
        qe.Property1 <- "b1"
        datasync.Update( qe ) |> should not' (be NullOrEmptyString)
        let qe1 = query "b1"
        qe1 |> should not' (be Null)
    
