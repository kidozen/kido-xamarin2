namespace Kido.iOS

open System
open System.Linq
open System.Threading.Tasks;

open MonoTouch.UIKit
open MonoTouch.Foundation

open HttpClient
open Utilities
open Kidozen
open Application
open System.Diagnostics

open Crash
open System.IO

// TODO: PCL Does not support File class. To delete, create , list, etc. I should implement
// my own PCL Module in kido.dll
module iOSCrash =
    let getDocumentsFolder = 
        match UIDevice.CurrentDevice.CheckSystemVersion(8,0) with
        | true -> 
            let url = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User)
            url.[0].Path
        | _ -> 
            let documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments)
            Path.GetFullPath (Path.Combine (documents, "..", "Library", "Caches"))

    let storeCrash crash = 
        let filename = String.Format("{0}.crash",  System.Guid.NewGuid().ToString())
        let documents = getDocumentsFolder
        Trace.WriteLine("storing crash in: " + Path.Combine (documents,filename))
        File.WriteAllText(Path.Combine (documents,filename), crash)

    let getCrashPendingToSend =  
        let documents = getDocumentsFolder
        List.ofSeq( System.IO.Directory.EnumerateFiles(documents,"*.crash"))

    let send crashpath mkp app key  = 
        Trace.WriteLine("sending crash file ...")
        let crash = File.ReadAllText crashpath
        let request = createCrashRequest crash mkp app key 
        let result = request |> getResult |> Async.RunSynchronously
        Trace.WriteLine("send crash result: " + result.StatusCode.ToString())
        match result.StatusCode with
        | 201 -> File.Delete crashpath
        | _ -> ()

    let processPending mkp app key = async {
        let messages = getCrashPendingToSend
        messages |> List.iter(fun(itm)-> send  itm mkp app key  ) 
        }

    let domainHandler =
        let handler (sender:obj) (e:UnhandledExceptionEventArgs) = 
            let ex = e.ExceptionObject :?> Exception
            let stack = new System.Diagnostics.StackTrace(ex,true)
            let frame = stack.GetFrame(0)
            let filename = frame.GetFileName()
            let linenumber = frame.GetFileLineNumber()
            let methodname = frame.GetMethod().Name
            let classname = frame.GetMethod().DeclaringType.FullName
            let fullstack = ex.StackTrace.Replace("\n",String.Empty)
            let reason = ex.GetType().Name
            let appVersionCode = NSBundle.MainBundle.InfoDictionary.["CFBundleShortVersionString"].ToString()

            let message = crashInformation "monotouch" 
                                            UIDevice.CurrentDevice.Name 
                                            UIDevice.CurrentDevice.SystemName 
                                            UIDevice.CurrentDevice.SystemVersion 
                                            filename 
                                            linenumber 
                                            methodname 
                                            classname 
                                            fullstack
                                            reason
                                            appVersionCode
                                            appVersionCode


            storeCrash  message 
        new UnhandledExceptionEventHandler(handler)
