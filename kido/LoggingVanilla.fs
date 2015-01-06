namespace Kidozen

open Logging
open System
open Newtonsoft.Json
open System.IO
open System.Linq
open System.Collections.Generic
open Serialization
open HttpClient
open Utilities
open identityproviders
open KzApplication
open System.Runtime.InteropServices

type LogLevel = 
    Verbose = 0 
    | Information = 1 
    | Warning = 2 
    | Error =3 
    | Critical = 4

type Log (identity:Identity) = 
    let mutable logidentity = identity

    let getKeyToken marketplace application key = async {
        let! result = asyncGetKeyToken marketplace application key
        match result with
            | Token t -> return t
            | InvalidApplication e -> return raise e
            | InvalidCredentials e -> return raise e
            | InvalidIpCredentials e -> return raise e
    }

    //
    member this.Create (message, data, level:LogLevel, marketplace, application, key) =
        let lvl = (int)level
        let entity = LogEntityParam (JSONSerializer.toString (data))
        let create = async {
            //this method could be called before you call authenticate
            let! createIdentity = 
                match identity.rawToken with
                    | "" -> getKeyToken marketplace application key
                    | _ -> async {return logidentity}

            let! result = createLogRequest createIdentity  |> withMessage message  |> withLevel lvl |> withLogOperation WriteLog |> withParameters entity |> getResult
            return 
                match result.StatusCode with
                    | 201 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask
    
    member this.Clear () = 
        let delete = async {
            let! result = createLogRequest logidentity |> withLogOperation DropLog |> getResult
            return
                match result.StatusCode with
                    | 204 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        delete |> Async.StartAsTask

    /// Query and returns an string representation
    member this.Query (query) =
        let queryParams = LogQueryParams [{name="query"; value=query}] 
        let query = async {
            let! result = createLogRequest logidentity |> withLogOperation QueryLog |> withParameters queryParams  |> getResult 
            return match result.StatusCode with
                    | 200 -> result.EntityBody.Value
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask

    /// Query and returns an IEnumerable<T>
    member this.Query<'a>(query) =
        let queryParams = LogQueryParams [{name="query"; value=query}] 
        let query = async {
            let! result = createLogRequest logidentity |> withLogOperation QueryLog |> withParameters queryParams  |> getResult 
            return match result.StatusCode with
                    | 200 -> JsonConvert.DeserializeObject<IEnumerable<'a>>(result.EntityBody.Value)
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }
        query |> Async.StartAsTask
            /// Query and returns an string representation

