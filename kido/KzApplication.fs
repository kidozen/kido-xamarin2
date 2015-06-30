module KzApplication

open HttpClient
open Utilities
open identityproviders
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open System.Collections.Generic 

[<assembly:InternalsVisibleTo("Kidozen.iOS")>]
do()

// ** Configuration
let appConfigurationCache = ref Map.empty

type GetConfigurationResult = 
    | Configuration of string
    | InvalidApplicationError of System.ArgumentException

let getCachedResponse f =
    fun x ->
        match (!appConfigurationCache).TryFind(x) with
        | Some res -> res
        | None ->
             let res = f x
             appConfigurationCache := (!appConfigurationCache).Add(x,res)
             res

let getAppConfig cfgurl = 
    let cachedResponse = getCachedResponse(fun url -> createRequest Get url) 
    let response = cachedResponse(cfgurl) |> getResponse
    match response.StatusCode  with
        | 200 -> 
            match response.EntityBody.IsSome with
                | true -> 
                    match response.EntityBody.Value with
                    | "[]" -> InvalidApplicationError (new System.ArgumentException("application does not exits"))
                    | _ -> Configuration (response.EntityBody.Value)
                | _ -> InvalidApplicationError (new System.ArgumentException("application does not exits"))
        | 404 -> InvalidApplicationError (new System.ArgumentException("application does not exits"))
        | _  -> InvalidApplicationError (new System.ArgumentException( sprintf "invalid status code = %i" response.StatusCode ))
               
let createConfigUrl marketplace application =
    match  marketplace with
        | Prefix "http://" rest -> sprintf "%s/publicapi/apps?name=%s" marketplace application
        | Prefix "https://" rest -> sprintf "%s/publicapi/apps?name=%s" marketplace application
        | _ -> sprintf "https://%s/publicapi/apps?name=%s" marketplace application
                 
let asyncGetApplicationConfig fAsyncGetAppConfig = 
    async {
        let! result = fAsyncGetAppConfig
        return result 
    }

// ** Authentication & Identity
type ProviderAuthenticationRequest = {
    Key : string 
    User : string
    Password : string
    }

type AuthenticationRequest = {
    Marketplace : string
    Application : string
    Key : string
    ProviderRequest : ProviderAuthenticationRequest option
    }

type Identity = {
    id : string
    rawToken : string
    token : Token option
    config : string
    expiration : System.DateTime 
    authenticationRequest : AuthenticationRequest
    }

type GetTokenResult = 
    | Token of Identity
    | InvalidApplication of System.ArgumentException
    | InvalidIpCredentials of System.ArgumentException
    | InvalidCredentials of System.ArgumentException

let getExpiration token =   
    let m = Regex.Match(token, "(?s)(?<=ExpiresOn=).+?(?=&)") in //ExpiresOn=(.*)&
    match m.Success with 
        | true ->
            let seconds = System.Double.Parse (m.Value)
            let epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
            let expiresOn = epoch.AddSeconds(seconds) 
            expiresOn
        | false -> System.DateTime.Now

let asyncGetFederatedToken marketplace application user password provider = 
    async {
        let appConfig = getAppConfig (createConfigUrl marketplace application)
        match appConfig with
        | Configuration c ->
            let providercfg = getJsonObjectValue c provider
            let domain = getJsonStringValue c "domain"
            let ipScope = getJsonStringValue c "authServiceScope"
            let ipEndpoint = getJsonStringValue providercfg.Value "endpoint"
            let protocol = getJsonStringValue providercfg.Value "protocol"

            let ipToken = 
                match protocol.Value.ToLower() with
                | "ws-trust" -> getWSTrustToken ipEndpoint.Value ipScope.Value user password
                | _ -> getWrapv9Token ipEndpoint.Value ipScope.Value user password //default is Wrapv09

            match ipToken with
                | Some t ->
                    let kidoAppScope = getJsonStringValue c "applicationScope"
                    let kidoEndpoint = getJsonStringValue c "authServiceEndpoint"
                    let encodedScope = urlEncode kidoAppScope.Value
                    let encodedtoken = urlEncode ipToken.Value 
                    let kidoToken = getKidoFederatedToken kidoEndpoint.Value encodedScope encodedtoken
                    match kidoToken.raw with
                        | Some t -> 
                            match keyValueStringToDictionary t with
                                | Some kvd -> 
                                    match kvd.["http://schemas.kidozen.com/usersource"] with
                                        | null -> return InvalidCredentials (new System.ArgumentException("You dont have access to this resource"))
                                        | _ -> 
                                            let userid = kvd.["http://schemas.kidozen.com/userid"]
                                            let provider= { User = user; Password = password; Key = provider }
                                            let request = { Key = ""; ProviderRequest = Some( provider ) ; Marketplace = marketplace; Application = application } 
                                            return Token( { id = userid; config = c; token = Some ( kidoToken ) ;rawToken = t ; expiration = getExpiration t; authenticationRequest = request})                                    
                                | _ -> return InvalidCredentials( new System.ArgumentException("Could not get user tokens"))
                        | _ -> return InvalidCredentials (new System.ArgumentException("kidozen couldnt validate ip token, please check the username, password and identity provider"))
                | _ -> return InvalidIpCredentials (new System.ArgumentException("invalid ip response, please check the username, password and identity provider"))

        | InvalidApplicationError e -> return InvalidApplication e
    }

let asyncGetKeyToken marketplace application key = 
    async {
        let appConfig = getAppConfig (createConfigUrl marketplace application)
        match appConfig with
        | Configuration c ->
            let domain = getJsonStringValue c "domain"
            let appScope = getJsonStringValue c "applicationScope"
            let kidoEndpoint = getJsonStringValue c "oauthTokenEndpoint"
            let kidoToken = getKidoKeyToken kidoEndpoint.Value appScope.Value domain.Value key
            match kidoToken.raw with
                | Some t -> 
                    match keyValueStringToDictionary t with
                        | Some kvd -> 
                            match kvd.["http://schemas.kidozen.com/usersource"] with
                                | null -> return InvalidCredentials (new System.ArgumentException("You dont have access to this resource"))
                                | _ -> 
                                    let userid = kvd.["http://schemas.kidozen.com/userid"]
                                    let request =  { Key = key; ProviderRequest = None; Marketplace = marketplace; Application = application }
                                    return Token( { id =userid; config = c; token = Some ( kidoToken ); rawToken = t;expiration = getExpiration t;  authenticationRequest = request} )
                        | _ -> return InvalidCredentials( new System.ArgumentException("Could not get user tokens"))
                | _ -> return InvalidCredentials (new System.ArgumentException("invalid application key"))
        | InvalidApplicationError e -> return InvalidApplication e
    }

let asyncGetKidoRefreshToken authRequest =
    System.Diagnostics.Debug.WriteLine("refresh auth token")
    async {
        let kidoEndpoint = getJsonStringValue authRequest.config "oauthTokenEndpoint"
        let domain = getJsonStringValue authRequest.config "domain"
        let refresh = getKidoRefreshToken kidoEndpoint.Value authRequest.token.Value.refresh.Value domain.Value authRequest.authenticationRequest.Key authRequest.authenticationRequest.Application
        match refresh.raw with
            | Some t -> 
                match keyValueStringToDictionary t with
                    | Some kvd -> 
                        match kvd.["http://schemas.kidozen.com/usersource"] with
                            | null -> return InvalidCredentials (new System.ArgumentException("You dont have access to this resource"))
                            | _ -> 
                                let userid = kvd.["http://schemas.kidozen.com/userid"]
                                return Token( { id =userid; config = authRequest.config ; token = Some ( refresh ); rawToken = t;expiration = getExpiration t;  authenticationRequest = authRequest.authenticationRequest} )
                        | _ -> return InvalidCredentials( new System.ArgumentException("Could not get user tokens"))
                | _ -> return InvalidCredentials (new System.ArgumentException("invalid application key"))
    }    

let validateToken authRequest = 
    async {
        let authrequest = authRequest.authenticationRequest
        let now = System.DateTime.Now
        let compareResult = System.DateTime.Compare(authRequest.expiration.ToUniversalTime(), now.ToUniversalTime())
        match compareResult with
            | -1 | 0 -> // expired or near to expire
                let! identity = 
                    match authrequest.ProviderRequest  with
                    | Some pr -> asyncGetFederatedToken authrequest.Marketplace authrequest.Application authrequest.ProviderRequest.Value.User authrequest.ProviderRequest.Value.Password authrequest.ProviderRequest.Value.Key
                    | _ -> asyncGetKidoRefreshToken authRequest
                match identity with
                    | Token t -> return t
                    | InvalidApplication e -> return raise e
                    | InvalidCredentials e -> return raise e
                    | InvalidIpCredentials e -> return raise e
            | _ -> return authRequest
    }

//passive auth and other forms support to be called from c#
let fetchConfigValue name marketplace application key = 
    let valuetask = async {
        let! result = asyncGetKeyToken marketplace application key
        match result with
            | Token t -> 
                   let appConfig = getAppConfig (createConfigUrl marketplace application)
                   match appConfig with
                       | Configuration c -> 
                           let signinurl = getJsonStringValue c name
                           match signinurl with
                               | Some v -> return v
                               | None -> 
                                    let message = sprintf "value not found: %s" name
                                    return raise (System.ArgumentException(message))
                       | InvalidApplicationError e -> return raise e
            | InvalidApplication e -> return raise e
            | InvalidCredentials e -> return raise e
            | InvalidIpCredentials e -> return raise e
    }
    valuetask |> Async.StartAsTask

