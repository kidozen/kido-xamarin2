module identityproviders

open System
open System.Collections.Generic 

open HttpClient
open Utilities

// IP's tokens
// TODO: change Some / None for detail error
let getWrapv9Token ipEndpoint scope name secret =
    let body = sprintf "wrap_name=%s&wrap_password=%s&wrap_scope=%s" name secret scope 
    let content = createRequest Post ipEndpoint |> withBody body |> withHeader (ContentType "application/x-www-form-urlencoded") |> getResponseBody   
    match content with
    | Match "<t:RequestedSecurityToken(?:\s+[^>]+)?>(.*?)<\/t:RequestedSecurityToken>" result -> Some(result.Head)
    | _ -> None

let getWSTrustToken ipEndpoint scope name secret =
    let body = "<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://www.w3.org/2005/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"><s:Header><a:Action s:mustUnderstand=\"1\">http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue</a:Action><a:To s:mustUnderstand=\"1\">" + ipEndpoint + "</a:To><o:Security s:mustUnderstand=\"1\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:UsernameToken u:Id=\"uuid-6a13a244-dac6-42c1-84c5-cbb345b0c4c4-1\"><o:Username>" + name + "</o:Username><o:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">" + secret + "</o:Password></o:UsernameToken></o:Security></s:Header><s:Body><trust:RequestSecurityToken xmlns:trust=\"http://docs.oasis-open.org/ws-sx/ws-trust/200512\"><wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\"><a:EndpointReference><a:Address>" + scope + "</a:Address></a:EndpointReference></wsp:AppliesTo><trust:KeyType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer</trust:KeyType><trust:RequestType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue</trust:RequestType><trust:TokenType>urn:oasis:names:tc:SAML:2.0:assertion</trust:TokenType></trust:RequestSecurityToken></s:Body></s:Envelope>";
    let content = createRequest Post ipEndpoint |> withBody body |> withHeader (ContentType "application/soap+xml;charset=UTF-8") |> getResponseBody   
    match content with
    | Match "<t:RequestedSecurityToken(?:\s+[^>]+)?>(.*?)<\/t:RequestedSecurityToken>" result -> Some(result.Head)
    | _ -> None

let getKidoFederatedToken endpoint scope token =
    let body = sprintf "wrap_assertion_format=SAML&wrap_assertion=%s&wrap_scope=%s" token scope
    let content = createRequest Post endpoint |> withBody body |> withHeader (ContentType "application/x-www-form-urlencoded") |> getResponseBody   
    let rawToken = getJsonStringValue content "rawToken"
    let expirationTime = getJsonStringValue content "expirationTime"
    let refreshToken = getJsonStringValue content "refresh_token"
    { raw = rawToken; expiration = expirationTime; refresh = refreshToken } 

let getKidoKeyToken ipEndpoint scope domain applicationKey =
    let body = sprintf "{\"client_id\":\"%s\",\"client_secret\":\"%s\",\"grant_type\":\"client_credentials\",\"scope\":\"%s\"}" domain applicationKey scope 
    let content = createRequest Post ipEndpoint |> withHeader (ContentType "application/json") |> withHeader (Accept "application/json") |> withBody body |> getResponseBody   
    let rawToken = getJsonStringValue content "access_token"
    { raw = rawToken; expiration = None; refresh = None }

let getKidoRefreshToken ipEndpoint refreshToken domain applicationKey =
    let body = sprintf "{\"client_id\":\"%s\",\"client_secret\":\"%s\",\"grant_type\":\"refresh_token\",\"refresh_token\":\"%s\"}" domain applicationKey refreshToken
    let content = createRequest Post ipEndpoint |> withHeader (ContentType "application/json") |> withHeader (Accept "application/json") |> withBody body |> getResponseBody   
    let rawToken = getJsonStringValue content "access_token"
    { raw = rawToken; expiration = None; refresh = None }