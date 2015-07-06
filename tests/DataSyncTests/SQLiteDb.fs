module SQLiteDb

open System
open System.IO
open System.Threading
open System.Net
open System.Linq
open System.Diagnostics

open SQLitePCL
open SQLitePCL.Ugly
open NUnit
open NUnit.Framework
open FsUnit

let getfakeresponse httpmethod db endpoint parameters  = 
    let currentdir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory));
    let query = sprintf "SELECT response FROM SyncResponses WHERE httpmethod='%s' AND db = '%s' AND endpoint = '%s' AND params = '%s'" httpmethod db endpoint parameters
    let mutable currentdb:sqlite3 = null;
    let result = raw.sqlite3_open(Path.Combine(currentdir, "FakeServerfsData.sqlite"), &currentdb)
    let value = match result with
                | 0 -> currentdb.query_one_column<string>(query).FirstOrDefault() 
                | _ -> failwith "Could not open db" 
    currentdb.close |> ignore 
    value

[<TestFixture>]
type DbTest()=
    [<Test>]
    member this.Query() =
        let r = getfakeresponse "GET" "one" "a" "none"
        r |> should equal "hola"