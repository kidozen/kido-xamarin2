
module Application

open HttpClient
open Utilities
open identityproviders
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open System.Collections.Generic 


[<assembly:InternalsVisibleTo("Kidozen.iOS")>]
do()

// ** Configuration
type GetConfigurationResult = 
    | Configuration of string
    | InvalidApplicationError of System.ArgumentException

let getAppConfig cfgurl = 
    let response = createRequest Get cfgurl |> getResponse
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
               

//let memGetAppConfig = memoize getAppConfig //memoizes the configuration fetch to cache it

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
let memGetFederatedToken = memoize getWrapv9Token

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
            let protocol = getJsonStringValue c "protocol"

            // todo: use partial application to create differents logins ?
            let ipToken = 
                match protocol.Value with
                | "WSTrust" -> getWSTrustToken ipEndpoint.Value ipScope.Value user password
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
                            let provider= { User = user; Password = password; Key = provider }
                            let request = { Key = ""; ProviderRequest = Some( provider ) ; Marketplace = marketplace; Application = application } 
                            return Token( { id = "1"; config = c; token = Some ( kidoToken ) ;rawToken = t ; expiration = getExpiration t; authenticationRequest = request})
                        | _ -> return InvalidCredentials (new System.ArgumentException("kidozen couldnt validate ip token, please check the username, password and identity provider"))

                | _ -> return InvalidIpCredentials (new System.ArgumentException("invalid ip response, please check the username, password and identity provider"))

        | InvalidApplicationError e -> 
            return InvalidApplication e
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
                    let request =  { Key = key; ProviderRequest = None; Marketplace = marketplace; Application = application }
                    return Token( { id = "2"; config = c; token = Some ( kidoToken ); rawToken = t;expiration = getExpiration t;  authenticationRequest = request} )
                | _ -> return InvalidCredentials (new System.ArgumentException("invalid application key"))
                       
        | InvalidApplicationError e -> 
            return InvalidApplication e
    }
                
let asyncGeToken fAsyncGetToken = 
    async {
        let! result = fAsyncGetToken
        return result
    }

let validateToken authRequest = 
    async {
        let authrequest = authRequest.authenticationRequest
        match System.DateTime.Compare(authRequest.expiration,System.DateTime.Now) with
            | -1 | 0 -> // expired or near to expire
                let! identity = 
                    match authrequest.ProviderRequest  with
                    | Some pr -> asyncGetFederatedToken authrequest.Marketplace authrequest.Application authrequest.ProviderRequest.Value.User authrequest.ProviderRequest.Value.Password authrequest.ProviderRequest.Value.Key
                    | _ -> asyncGetKeyToken authrequest.Marketplace authrequest.Application authrequest.Key
                match identity with
                    | Token t -> return t
                    | InvalidApplication e -> return raise e
                    | InvalidCredentials e -> return raise e
                    | InvalidIpCredentials e -> return raise e
            | _ -> return authRequest
    }