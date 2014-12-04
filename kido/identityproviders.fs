module identityproviders

open System
open System.Collections.Generic 

open HttpClient
open Utilities

// cache: url is the key, tuple of date(expiration) and the token
let tokensCache = Dictionary<string, DateTime * string>()

let getToken f =
    fun x ->
        match tokensCache.TryGetValue(x) with
        | true, res -> snd res
        | false, _ -> 
            let res = f x
            tokensCache.Add(x, res)
            snd res


// IP's tokens
// TODO: change Some / None for detail error
let getWrapv9Token ipEndpoint scope name secret =
    let body = sprintf "wrap_name=%s&wrap_password=%s&wrap_scope=%s" name secret scope 
    let content = createRequest Post ipEndpoint |> withBody body |> withHeader (ContentType "application/x-www-form-urlencoded") |> getResponseBody   
    match content with
    | Match "<t:RequestedSecurityToken(?:\s+[^>]+)?>(.*?)<\/t:RequestedSecurityToken>" result -> Some(result.Head)
    | _ -> None

//todo: implement WSTrust
let getWSTrustToken ipEndpoint scope credentials = getWrapv9Token ipEndpoint scope credentials


let getKidoFederatedToken endpoint scope token =
    let body = sprintf "wrap_assertion_format=SAML&wrap_assertion=%s&wrap_scope=%s" token scope
    let content = createRequest Post endpoint |> withBody body |> withHeader (ContentType "application/x-www-form-urlencoded") |> getResponseBody   
    let rawToken = getJsonStringValue content "rawToken"
    let expirationTime = getJsonStringValue content "expirationTime"
    let refreshToken = getJsonStringValue content "refresh_token"
    {
        raw = rawToken;
        expiration = expirationTime;
        refresh = refreshToken
    } 

let getKidoKeyToken ipEndpoint scope domain applicationKey =
    let body = sprintf "{\"client_id\":\"%s\",\"client_secret\":\"%s\",\"grant_type\":\"client_credentials\",\"scope\":\"%s\"}" domain applicationKey scope 
    let content = createRequest Post ipEndpoint |> withHeader (ContentType "application/json") |> withHeader (Accept "application/json") |> withBody body |> getResponseBody   
    let rawToken = getJsonStringValue content "access_token"
    { raw = rawToken; expiration = None; refresh = None }
