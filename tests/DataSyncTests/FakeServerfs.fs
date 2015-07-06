module FakeServer

open Nancy
open Nancy.Extensions
open Nancy.Hosting.Self
open System
open System.Threading
open System.Text
open SQLiteDb

// A Nancy Response overridden to allow different encoding on the body
type EncodedResponse(body:string, encoding:string) =
    inherit Nancy.Response()
    let writeBody (stream:IO.Stream) = 
        let bytes = Encoding.GetEncoding(encoding).GetBytes(body)
        stream.Write(bytes, 0, bytes.Length)
    do base.Contents <- Action<IO.Stream> writeBody

// ? operator to get values from a Nancy DynamicDictionary
let (?) (parameters:obj) param =
    (parameters :?> Nancy.DynamicDictionary).[param]
 
let recordedRequest = ref (null:Request)

let addcouchheaders (response:Nancy.Response) =
    response.ContentType <- "text/plain; charset=utf-8"
    response.Headers.Add("server", "CouchDB/1.6.1 (Erlang OTP/R16B03)")
    response.Headers.Add("date", DateTime.Now.ToUniversalTime().ToShortTimeString())
    response.Headers.Add("cache-control", "must-revalidate")
    response

let requestAsArray (request:Nancy.Request) = request.Url.Path.Split('/')

let getlasturlpart (request:Nancy.Request) =
    let parts = requestAsArray request
    parts.[parts.Length-1]

type FakeServer() as self = 
    inherit NancyModule()
    do
        self.Post.["/wrapv0.9"] <- 
            fun _ -> 
                let xmlresponse = "<t:RequestSecurityTokenResponse xmlns:t=\"http://schemas.xmlsoap.org/ws/2005/02/trust\"><t:Lifetime><wsu:Created xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">2014-08-15T23:00:14.049Z</wsu:Created><wsu:Expires xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">2014-08-16T00:00:14.049Z</wsu:Expires></t:Lifetime><wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\"><EndpointReference xmlns=\"http://www.w3.org/2005/08/addressing\"><Address>http://auth.kidozen.com/</Address></EndpointReference></wsp:AppliesTo><t:RequestedSecurityToken><Assertion ID=\"_d56485cc-aa34-47ae-a28d-8c8357cdbece\" IssueInstant=\"2014-08-15T23:00:14.049Z\" Version=\"2.0\" xmlns=\"urn:oasis:names:tc:SAML:2.0:assertion\"><Issuer>https://identity.kidozen.com/</Issuer><ds:Signature xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><ds:SignedInfo><ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\" /><ds:SignatureMethod Algorithm=\"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256\" /><ds:Reference URI=\"#_d56485cc-aa34-47ae-a28d-8c8357cdbece\"><ds:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /><ds:Transform Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\" /></ds:Transforms><ds:DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" /><ds:DigestValue>MKesKYDObD3fFB5c4Cs8kVCoFI+BSSIWREwkyxWr288=</ds:DigestValue></ds:Reference></ds:SignedInfo><ds:SignatureValue>c7Oo7PnGtRHbXPgXCaogfuO4UKNW6vGqnT/oklAtzf9jz9CVuOmL2dN35yu0k8v5KALjPHbqKttQ93cbXbZIwXKNnrnUELq3rWPpqUSCiFNTqsNYX76+WD7rJAhfC+wrrELhqBZtU9TkakJYO9evAAZ/KtWgGN8qRNyhrbntl6I=</ds:SignatureValue><KeyInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><X509Data><X509Certificate>MIICDzCCAXygAwIBAgIQVWXAvbbQyI5BcFe0ssmeKTAJBgUrDgMCHQUAMB8xHTAbBgNVBAMTFGlkZW50aXR5LmtpZG96ZW4uY29tMB4XDTEyMDcwNTE4NTEzNFoXDTM5MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUaWRlbnRpdHkua2lkb3plbi5jb20wgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAJ1GPvzmIZ5OO5by9Qn2fsSuLIJWHfewRzgxcZ6SykzmjD4H1aGOtjUg5EFgQ/HWxa16oJ+afWa0dyeXAiLl5gas71FzgzeODL1STIuyLXFVLQvIJX/HTQU+qcMBlwsscdvVaJSYQsI3OC8Ny5GZvt1Jj2G9TzMTg2hLk5OfO1zxAgMBAAGjVDBSMFAGA1UdAQRJMEeAEDSvlNc0zNIzPd7NykB3GAWhITAfMR0wGwYDVQQDExRpZGVudGl0eS5raWRvemVuLmNvbYIQVWXAvbbQyI5BcFe0ssmeKTAJBgUrDgMCHQUAA4GBAIMmDNzL+Kl5omgxKRTgNWMSZAaMLgAo2GVnZyQ26mc3v+sNHRUJYJzdYOpU6l/P2d9YnijDz7VKfOQzsPu5lHK5s0NiKPaSb07wJBWCNe3iwuUNZg2xg/szhiNSWdq93vKJG1mmeiJSuMlMafJVqxC6K5atypwNNBKbpJEj4w5+</X509Certificate></X509Data></KeyInfo></ds:Signature><Subject><SubjectConfirmation Method=\"urn:oasis:names:tc:SAML:2.0:cm:bearer\" /></Subject><Conditions NotBefore=\"2014-08-15T23:00:14.049Z\" NotOnOrAfter=\"2014-08-16T00:00:14.049Z\"><AudienceRestriction><Audience>http://auth.kidozen.com/</Audience></AudienceRestriction></Conditions><AttributeStatement><Attribute Name=\"http://schemas.kidozen.com/domain\"><AttributeValue>kidozen.com</AttributeValue></Attribute><Attribute Name=\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name\"><AttributeValue>LoadTests Admin</AttributeValue></Attribute><Attribute Name=\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress\"><AttributeValue>loadtests@kidozen.com</AttributeValue></Attribute></AttributeStatement></Assertion></t:RequestedSecurityToken><t:RequestedAttachedReference><SecurityTokenReference d3p1:TokenType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0\" xmlns:d3p1=\"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd\" xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><KeyIdentifier ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID\">_d56485cc-aa34-47ae-a28d-8c8357cdbece</KeyIdentifier></SecurityTokenReference></t:RequestedAttachedReference><t:RequestedUnattachedReference><SecurityTokenReference d3p1:TokenType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0\" xmlns:d3p1=\"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd\" xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><KeyIdentifier ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID\">_d56485cc-aa34-47ae-a28d-8c8357cdbece</KeyIdentifier></SecurityTokenReference></t:RequestedUnattachedReference><t:TokenType>urn:oasis:names:tc:SAML:2.0:assertion</t:TokenType><t:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</t:RequestType><t:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</t:KeyType></t:RequestSecurityTokenResponse>"
                let response = new EncodedResponse(xmlresponse, "utf-8")
                response.ContentType <- "text/xml"
                response.StatusCode <- HttpStatusCode.OK 
                response :> obj
        self.Post.["kidoauth/key/wrapv0.9"] <- 
            fun _ -> 
                let jsonresponse = "{\"access_token\": \"http%3A%2F%2Fschemas.xmlsoap.org%2Fws%2F2005%2F05%2Fidentity%2Fclaims%2Femailaddress=bellhowelldev.kidocloud.com%40sda2&http%3A%2F%2Fschemas.xmlsoap.org%2Fws%2F2005%2F05%2Fidentity%2Fclaims%2Fname=bellhowelldev.kidocloud.com&http%3A%2F%2Fschemas.kidozen.com%2Fname=bellhowelldev.kidocloud.com&http%3A%2F%2Fschemas.kidozen.com%2Femail=bellhowelldev.kidocloud.com%40sda2&http%3A%2F%2Fschemas.kidozen.com%2Frole=Service%20Account&http%3A%2F%2Fschemas.kidozen.com%2Fusersource=Service%20Account&http%3A%2F%2Fschemas.kidozen.com%2Fidentityprovider=http%3A%2F%2Fauth.kidozen.com%2F&http%3A%2F%2Fschemas.kidozen.com%2Faction=allow%20create%20logging.*&http%3A%2F%2Fschemas.kidozen.com%2Fuserid=0afcdf49393e2ead2c7b68a5ec43ae6b&http%3A%2F%2Fschemas.kidozen.com%2Fauthmethod=client_credentials&http%3A%2F%2Fschemas.kidozen.com%2Fclientid=bellhowelldev.kidocloud.com&Audience=sda2&Issuer=http%3A%2F%2Fauth.kidozen.com%2F&ExpiresOn=1432322320&HMACSHA256=0em%2FYIoHXf%2B8AKvrNplgrRbm7SwlKAkw3ZJBeO52%2BcY%3D\",\"token_type\": \"Bearer\"}"
                let response = new EncodedResponse(jsonresponse, "utf-8")
                response.ContentType <- "application/json"
                response.StatusCode <- HttpStatusCode.OK 
                response :> obj
        self.Post.["kidoauth/wrapv0.9"] <- 
            fun _ -> 
                let jsonresponse = "{\"rawToken\":\"http%3A%2F%2Fschemas.kidozen.com%2Fdomain=kidozen.com&http%3A%2F%2Fschemas.xmlsoap.org%2Fws%2F2005%2F05%2Fidentity%2Fclaims%2Fname=LoadTests%20Admin&http%3A%2F%2Fschemas.xmlsoap.org%2Fws%2F2005%2F05%2Fidentity%2Fclaims%2Femailaddress=loadtests%40kidozen.com&http%3A%2F%2Fschemas.kidozen.com%2Fidentityprovider=https%3A%2F%2Fidentity.kidozen.com%2F&http%3A%2F%2Fschemas.kidozen.com%2Fuserid=8e90b7a88b4e910d8cda53db3e0793d9&http%3A%2F%2Fschemas.kidozen.com%2Fname=LoadTests%20Admin&http%3A%2F%2Fschemas.kidozen.com%2Femail=loadtests%40kidozen.com&http%3A%2F%2Fschemas.kidozen.com%2Fusersource=Admins%20(Kidozen)&http%3A%2F%2Fschemas.kidozen.com%2Frole=Application%20Admin&http%3A%2F%2Fschemas.kidozen.com%2Faction=allow%20all%20*&Audience=integration-tests&Issuer=http%3A%2F%2Fauth.kidozen.com%2F&ExpiresOn=1408133994&HMACSHA256=1v4dsPi3xLvBzzjnOOD2lEgtcfVmlj2es9MHLIlDoD0%3D\",\"refresh_token\":\"http%3A%2F%2Fschemas.kidozen.com%2Fdomain=kidozen.com&http%3A%2F%2Fschemas.xmlsoap.org%2Fws%2F2005%2F05%2Fidentity%2Fclaims%2Fname=LoadTests%20Admin&http%3A%2F%2Fschemas.xmlsoap.org%2Fws%2F2005%2F05%2Fidentity%2Fclaims%2Femailaddress=loadtests%40kidozen.com&http%3A%2F%2Fschemas.kidozen.com%2Fidentityprovider=https%3A%2F%2Fidentity.kidozen.com%2F&http%3A%2F%2Fschemas.kidozen.com%2Fuserid=8e90b7a88b4e910d8cda53db3e0793d9&http%3A%2F%2Fschemas.kidozen.com%2Fname=LoadTests%20Admin&http%3A%2F%2Fschemas.kidozen.com%2Femail=loadtests%40kidozen.com&http%3A%2F%2Fschemas.kidozen.com%2Fusersource=Admins%20(Kidozen)&http%3A%2F%2Fschemas.kidozen.com%2Frole=Application%20Admin&http%3A%2F%2Fschemas.kidozen.com%2Faction=allow%20all%20*&Audience=integration-tests&Issuer=http%3A%2F%2Fauth.kidozen.com%2F&ExpiresOn=1408133994&HMACSHA256=1v4dsPi3xLvBzzjnOOD2lEgtcfVmlj2es9MHLIlDoD0%3D\",\"expirationTime\":\"2014-08-15T20:19:54.355Z\"}"
                let response = new EncodedResponse(jsonresponse, "utf-8")
                response.ContentType <- "application/json"
                response.StatusCode <- HttpStatusCode.OK 
                response :> obj
        self.Get.["/publicapi/apps"] <- 
            fun _ -> 
                let appname = self.Request.Query?name
                let response = "[{\"displayName\":\"tasks\",\"domain\":\"localhost:1234\",\"name\":\"tasks\",\"url\":\"http://localhost:1234/\",\"datasync\":\"http://localhost:1234/api/v2/datasync\",\"authConfig\":{\"applicationScope\":\"tasks\",\"authServiceScope\":\"http: //auth.kidozen.com/\",\"authServiceEndpoint\":\"http://localhost:1235/kidoauth/wrapv0.9\",\"wrapEndpoint\":\"http://localhost:1235/kidoauth/wrapv0.9\", \"oauthTokenEndpoint\":\"http://localhost:1235/kidoauth/key/wrapv0.9\", \"identityProviders\":{\"Kidozen\":{\"name\":\"http: //identity.kidozen.com/\",\"protocol\":\"wrapv0.9\",\"endpoint\":\"http://localhost:1235/wrapv0.9\"}}}}]" |> Nancy.Response.op_Implicit 
                response.StatusCode <- HttpStatusCode.OK
                response :> obj

        //pull replication
        self.Get.["/api/v2/datasync/{db}"] <-
            fun _ ->
                let response = "OK"  |> Nancy.Response.op_Implicit  |> addcouchheaders
                response.ContentType <- "text/plain"
                response.StatusCode <- HttpStatusCode.OK
                response :> obj
        self.Get.["/api/v2/datasync/{db}/_local/(?<all>.*)"] <- 
            fun _ -> 
                let db = requestAsArray(self.Request).[4]
                let body =  getfakeresponse "GET" db "_local" "(?<all>.*)"
                let response = body |> Nancy.Response.op_Implicit  |> addcouchheaders
                response.ContentType <- "application/json"
                response.StatusCode <- HttpStatusCode.NotFound
                response :> obj
        self.Get.["/api/v2/datasync/{db}/_changes"] <- 
             fun _ -> 
                let db = requestAsArray(self.Request).[4]
                let body = getfakeresponse "GET" db "_changes" "none"
                let response = body |> Nancy.Response.op_Implicit  |> addcouchheaders
                response.StatusCode <- HttpStatusCode.OK
                response :> obj
        self.Get.["/api/v2/datasync/{db}/(?<all>.*)"] <- 
             fun _ -> 
                let db = requestAsArray(self.Request).[4]
                let id = getlasturlpart self.Request
                let body =  getfakeresponse "GET" db "none" id
                let response = body |> Nancy.Response.op_Implicit  |> addcouchheaders
                response.StatusCode <- HttpStatusCode.OK
                response :> obj

        self.Put.["/api/v2/datasync/{db}/_local/00000000000000000000000000000001"] <- 
            fun _ -> 
                let therequest = self.Request
                let response = "{\"ok\":true,\"id\":\"_local/00000000000000000000000000000001\",\"rev\":\"0-1\"}" |> Nancy.Response.op_Implicit  |> addcouchheaders
                response.ContentType <- "application/json"
                response.StatusCode <- HttpStatusCode.Created
                response :> obj
    