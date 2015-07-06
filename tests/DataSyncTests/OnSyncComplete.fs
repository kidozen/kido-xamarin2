module OnSyncComplete
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

let syncExecuteDS (kido:KidoApplication) name = 
    let ds = kido.DataSync<myEntity>(name)
    ds |> should not' (be Null)
    ds.OnSynchronizationComplete.Add( fun x -> (printfn "onComplete"))
    ds.Synchronize(SynchronizationType.Pull)
    let result = async {
        let! args = Async.AwaitEvent(ds.OnSynchronizationComplete)
        return args.Details
    }
    result

type ``Given a valid KidoZen instance`` ()= 
    let kido = new KidoApplication(kidozenMarketplace,"tests","testAppkey")
    
    do
        ServicePointManager.ServerCertificateValidationCallback <- (fun _ _ _ _ -> true)

    [<TestFixtureSetUp>]
    member this.fixtureSetup() = 
        fakeKidozenHost.Start()
        fakeIpHost.Start()
        let u = kido.Authenticate("test","pass","Kidozen") |> Async.AwaitTask |> Async.RunSynchronously
        kido.IsAuthenticated |> should be True
        let deleteds name =
            let ds = kido.DataSync<myEntity>(name)
            ds.Drop()
        let dss = ["onlynews"; "updates"; "deletes"]
        List.iter(fun ds-> deleteds ds ) dss
        
        
    [<TestFixtureTearDown>]
    member this.fixtureTearDown() = 
        fakeKidozenHost.Stop()
        fakeIpHost.Stop()

    [<Test>] 
    member this.``Should pull and NewCount should equals 1`` ()=
        let count = syncExecuteDS kido "onlynews" |> Async.RunSynchronously
        count.NewCount |> should equal 1       

                
    [<Test>] 
    member this.``Should pull and UpdateCount should equals 1`` ()=
        let count = syncExecuteDS kido "updates" |> Async.RunSynchronously
        count.UpdateCount |> should equal 1

    [<Test>] 
    member this.``Should pull and RemoveCount should equals 1`` ()=
        let count = syncExecuteDS kido "deletes" |> Async.RunSynchronously
        count.RemoveCount |> should equal 1

