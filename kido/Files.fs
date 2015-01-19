namespace Kidozen

open System
open HttpClient
open Utilities
open identityproviders
open Serialization
open KzApplication
open System.Runtime.InteropServices

open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic

[<NoEquality;NoComparison>]
type File = {
    lastModified: DateTime;
    name: string;
    size: int64;
    }

[<NoEquality;NoComparison>]
type FilesBrowseResult = {
    path: string;
    files: IEnumerable<File>;
    subfolders:IEnumerable<string>;
}


type Files (identity:Identity) = 
    let baseurl =(getJsonStringValue (identity.config) "files" ).Value
    member this.identity = identity

    member this.Upload (bytes, fullpath:string) =
        let cleanFullpath = fullpath.TrimStart [|'/'|]
        let filename = cleanFullpath.Split[|'/'|] |> Array.toList |> List.rev |> List.head
        let path = cleanFullpath.Split[|'/'|] |> Array.toList |> List.rev |> List.tail |> List.rev |> String.concat "/"
        let url = sprintf "%s/%s/" baseurl path

        let service =  async {
            let! result = createRequest HttpMethod.Post url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> withHeader (Custom {name="x-file-name";value=filename}) 
                            |> withBodyAsBytes bytes
                            |> getResponseAsync 
            return 
                match result.StatusCode with
                    | 200 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask
   
    member this.Browse(fullpath:string) =
        let cleanFullpath = fullpath.TrimStart([|'/'|]).TrimEnd([|'/'|])
        let url = sprintf "%s/%s/" baseurl cleanFullpath

        let service =  async {
            let! result = createRequest HttpMethod.Get url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseAsync
            return 
                match result.StatusCode with
                    | 200 -> JsonConvert.DeserializeObject<FilesBrowseResult>(result.EntityBody.Value)
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask

    member this.Delete(fullpath:string) =
        let cleanFullpath = fullpath.TrimStart([|'/'|]).TrimEnd([|'/'|])
        let url = sprintf "%s/%s" baseurl cleanFullpath

        let service =  async {
            let! result = createRequest HttpMethod.Delete url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseAsync
            return 
                match result.StatusCode with
                    | 204 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask

    member this.DownloadAsBytes(fullpath:string) =
        let cleanFullpath = fullpath.TrimStart [|'/'|]
        let url = sprintf "%s/%s" baseurl cleanFullpath

        let service =  async {
            let! result = createRequest HttpMethod.Get url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseBytesAsync 
            return 
                match result.StatusCode with
                    | 200 -> 
                        match result.BytesBody with
                            | Some bytes -> bytes
                            | None -> raise ( new Exception("Service returns empty body. Please check your settings and try again") )      
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask

    //Helper method for other services such as DataVisualizations API
    member this.DownloadFromUrl(url:string) =
        let service =  async {
            let! result = createRequest HttpMethod.Get url  
                            |> withHeader (Authorization this.identity.rawToken) 
                            |> getResponseBytesAsync 
            return 
                match result.StatusCode with
                    | 200 -> 
                        match result.BytesBody with
                            | Some bytes -> bytes
                            | None -> raise ( new Exception("Service returns empty body. Please check your settings and try again") )      
                    | _ -> raise (new Exception (result.EntityBody.Value))

            }
        service |> Async.StartAsTask
