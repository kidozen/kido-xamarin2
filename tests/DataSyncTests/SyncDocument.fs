module SyncDocument
open System
open NUnit.Framework
open FsUnit

open Kidozen.DataSync

[<TestFixture>] 
type ``Given a SyncDocument instance`` ()=
    let doc = new SyncDocument<string>("hello")

    [<Test>] member this.
     ``Should not be null`` ()= doc |> should not' (be Null)
    [<Test>] member this.
     ``Should return the Document property`` ()= 
        doc.Document |> should equal "hello"