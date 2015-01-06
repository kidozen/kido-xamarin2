namespace Kidozen

open Configuration
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


type Configuration (name, identity:Identity) = 
    member this.name:string = name
    member this.identity = identity

    //
    member this.Save (parameters) =
        let entity = ConfigurationEntityParam (JSONSerializer.toString (parameters))
        let create = async {
                let! result = createConfigurationRequest this.name this.identity |> withConfigurationOperation CreateConfiguration |> withParameters entity |> getResult
                return 
                    match result.StatusCode with
                        | 201 -> true
                        | _ -> raise (new Exception (result.EntityBody.Value))
            }    
        create |> Async.StartAsTask

    //
    member this.Delete() = 
        let delete = async {
            let! result = createConfigurationRequest this.name this.identity |>  withConfigurationOperation DeleteConfiguration|> getResult
            return
                match result.StatusCode with
                    | 200 -> true
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        delete |> Async.StartAsTask
    
    /// Returns an instance of T
    member this.Get<'a>() =
        let get = async {
            let! result = createConfigurationRequest this.name this.identity |>  withConfigurationOperation GetConfiguration|> getResult
            return match result.StatusCode with
                    | 200 -> 
                        match result.ContentLength with
                            |  l when l > int64(0) -> JsonConvert.DeserializeObject<'a>(result.EntityBody.Value)
                            |  _ -> raise (new ArgumentException("Invalid configuration name",this.name))
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask
    
    /// Returns an object as JSON
    member this.Get() =
        let get = async {
            let! result = createConfigurationRequest this.name this.identity |> withConfigurationOperation GetConfiguration|> getResult
            return match result.StatusCode with
                    | 200 -> 
                        match result.ContentLength with
                            |  l when l > int64(0) -> result.EntityBody.Value
                            |  _ -> raise (new ArgumentException("Invalid configuration name",this.name))
                    | _ -> raise (new Exception (result.EntityBody.Value))
            }  
        get |> Async.StartAsTask
