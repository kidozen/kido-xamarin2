module Integration
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

type todoitem() =
    inherit DataSyncDocument()
    member val id = 0 with get,set
    member val Name = "" with get, set
    member val Notes = "" with get, set
    member val Done = false with get, set

type myEntity() = 
    inherit DataSyncDocument()
    member val Property1 = "" with get, set

type part() =
    inherit DataSyncDocument()
    member val PARTNUMBER = "" with get, set
    member val PARTNAME = "" with get, set
    member val PARTUOM  = "" with get, set
    member val PARTQUANTITY= 0 with get, set
    member val STORAGELOCATIONID = "" with get, set
    member val REQUIRESRMA = "" with get, set
    member val PLANT  = "" with get, set

type ``Given a valid KidoZen deployment`` ()= 
    //let kido = new KidoApplication("bellhowelldev.kidocloud.com","sda2","WLasDb3gRhTzzgpD4iQo82H1NIwQB7n7yxDk/d88PqE=")
    let kido = new KidoApplication("build.kidocloud.com","todolist","kyEcl5fSzTSta7lG3CcBZq1kk1IaT6shOOXt+6TVhWk=")
    do
        ServicePointManager.ServerCertificateValidationCallback <- (fun _ _ _ _ -> true)
    [<TestFixtureSetUp>]
    member this.fixtureSetup() = 
        //let ufn = kido.Authenticate("sdademo@bhemail.com","123456Sd","Bell and Howell ADFS") 
        let ufn = kido.Authenticate("build@kidozen.com","pass","Kidozen") 
        let u = ufn |> Async.AwaitTask |> Async.RunSynchronously
        kido.IsAuthenticated |> should be True
    [<Test>] 
    member this.``Should pull all documents`` ()=
        let datasync = kido.DataSync<todoitem>("todolist") //partinventorylookup
        datasync.OnSynchronizationComplete.Add( fun x -> (printfn "onComplete"))
        datasync.Synchronize(SynchronizationType.Pull)
        let result = async {
            let! args = Async.AwaitEvent(datasync.OnSynchronizationComplete)
            printfn "onComplete inside async"
            return args.Details
        }
        let count = result |> Async.RunSynchronously
        printfn "Total news: %d" count.NewCount
        printfn "Total deleted: %d" count.RemoveCount
        printfn "Total updated: %d" count.UpdateCount
        count |> should not' (be Null)

    [<Test>]
    member this.``Should push a document`` ()=  
        let datasync = kido.DataSync<todoitem>("todolist") //partinventorylookup
        datasync.OnSynchronizationComplete.Add( fun x -> (printfn "onComplete"))
        datasync.Synchronize(SynchronizationType.Push)
        let result = async {
            let! args = Async.AwaitEvent(datasync.OnSynchronizationComplete)
            printfn "onComplete inside async"
            return args.Details
        }
        let count = result |> Async.RunSynchronously
        printfn "Total news: %d" count.NewCount
        printfn "Total deleted: %d" count.RemoveCount
        printfn "Total updated: %d" count.UpdateCount
        count |> should not' (be Null)
        