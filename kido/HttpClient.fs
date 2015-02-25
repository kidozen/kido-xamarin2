module HttpClient
open System
open System.IO
open System.Net.Http
open System.Text
open Microsoft.FSharp.Control
open Microsoft.FSharp.Control.CommonExtensions
open Microsoft.FSharp.Control.WebExtensions
open System.Collections.Generic
open Utilities
open ModernHttpClient

let private ISO_Latin_1 = "ISO-8859-1"
let private UTF_8 = "utf-8"

let private XKidoSDK = "X-Kido-SDK"
let private XKidoSDKValue = "Xamarin"
let private XKidoSDKVersion = "X-Kido-SDK-Version"
let private XKidoSDKVersionValue = "2.0.2"

type HttpMethod = Options | Get | Head | Post | Put | Delete | Trace // Connect

// Same as System.Net.DecompressionMethods, but I didn't want to expose that
type DecompressionScheme = 
    | None = 0
    | GZip = 1
    | Deflate = 2

// Defines mappings between encodings which might be specified to the names
// which work with the .net encoder
let private responseEncodingMappings =
    Map.empty
        .Add("utf8", "utf-8")
        .Add("utf16", "utf-16")

type NameValue = { name:string; value:string }
type ContentRange = {start:int64; finish:int64 }

type ResponseHeader =
    | AccessControlAllowOrigin 
    | AcceptRanges 
    | Age 
    | Allow 
    | CacheControl 
    | Connection 
    | ContentEncoding 
    | ContentLanguage 
    | ContentLength
    | ContentLocation 
    | ContentMD5Response 
    | ContentDisposition 
    | ContentRange 
    | ContentTypeResponse 
    | DateResponse 
    | ETag 
    | Expires 
    | LastModified 
    | Link 
    | Location 
    | P3P 
    | PragmaResponse 
    | ProxyAuthenticate 
    | Refresh 
    | RetryAfter 
    | Server 
    | StrictTransportSecurity 
    | Trailer 
    | TransferEncoding 
    | Vary 
    | ViaResponse 
    | WarningResponse 
    | WWWAuthenticate 
    | NonStandard of string

// some headers can't be set with HttpNetClient, or are set automatically, so are not included.
// others, such as transfer-encoding, just haven't been implemented.
type RequestHeader =
    | Accept of string
    | AcceptCharset of string
    | AcceptDatetime of string
    | AcceptLanguage of string
    | Authorization of string
    | Connection of string
    | ContentMD5 of string
    | ContentType of string
    | Date of DateTime
    | Expect of int
    | From of string
    | IfMatch of string
    | IfModifiedSince of DateTime
    | IfNoneMatch of string
    | IfRange of string
    | MaxForwards of int
    | Origin of string
    | Pragma of string
    | ProxyAuthorization of string
    | Range of ContentRange
    | Referer of string
    | Upgrade of string
    | UserAgent of string
    | Via of string
    | Warning of string
    | Custom of NameValue

type UserDetails = { username:string; password:string }

[<RequireQualifiedAccess>]
type ProxyCredentials =
    | None
    | Default
    | Custom of UserDetails

type Proxy = { 
    Address: string
    Port: int
    Credentials: ProxyCredentials 
}

type Request = {
    Url: string
    Method: HttpMethod
    CookiesEnabled: bool
    AutoFollowRedirects: bool
    AutoDecompression: DecompressionScheme
    Headers: RequestHeader list option
    Body: string option
    BodyBytes : byte[] option
    BodyCharacterEncoding: string option
    QueryStringItems: NameValue list option
    Cookies: NameValue list option
    ResponseCharacterEncoding: string option
    Proxy: Proxy option
    SupportsTransferEncodingChunked :  bool option
    KeepAlive: bool
}

type Response = {
    StatusCode: int
    EntityBody: string option
    BytesBody: byte[] option
    ContentLength: int64
    Cookies: Map<string,string> option
    Headers: Map<ResponseHeader, string list>
}

/// <summary>Creates the Request record which can be used to make an HTTP request</summary>
/// <param name="httpMethod">The type of request to be made (Get, Post, etc.)</param>
/// <param name="url">The URL of the resource including protocol, e.g. 'http://www.kidozen.com'</param>
/// <returns>The Request record</returns>
let createRequest httpMethod url = {
    Url = url; 
    Method = httpMethod;
    CookiesEnabled = true;
    AutoFollowRedirects = true;
    AutoDecompression = DecompressionScheme.None;
    Headers = None; 
    Body = None;
    BodyBytes = None;
    BodyCharacterEncoding = None;
    QueryStringItems = None;
    Cookies = None;
    ResponseCharacterEncoding = None;
    Proxy = None;
    SupportsTransferEncodingChunked = None;
    KeepAlive = true;
}

// Checks if a header already exists in a list
// (standard headers just checks type, custom headers also checks 'name' field).
let private headerExists header headerList =
    headerList
    |> List.exists (
            fun existingHeader -> 
                match existingHeader, header with
                | Custom {name = existingName; value = existingValue },
                  Custom {name = newName; value = newValue } -> existingName = newName
                | _ -> existingHeader.GetType() = header.GetType())

// Checks if a header already exists in a list
// (standard headers just checks type, custom headers also checks 'name' field).
let private headerGet header headerList =
    let filtered = headerList |> List.filter (
                                    fun existingHeader -> 
                                        match existingHeader, header with
                                        | Custom {name = existingName; value = existingValue },
                                          Custom {name = newName; value = newValue } -> existingName = newName
                                        | _ -> existingHeader.GetType() = header.GetType()) 
            
    match filtered with
    |[] -> None
    |_ -> Some (filtered |> List.head )

// Adds a header to the collection as long as it isn't already in it
let private appendHeaderNoRepeat newHeader headerList =
    match headerList with
    | None -> Some([newHeader])
    | Some(existingList) -> 
        if existingList |> headerExists newHeader then
            failwithf "Header %A already exists" newHeader
        Some(existingList@[newHeader])


/// Disables automatic following of redirects, which is enabled by default
let withAutoFollowRedirectsDisabled request = 
    {request with AutoFollowRedirects = false }

/// Adds a header, defined as a RequestHeader
let withHeader header (request:Request) =
    {request with Headers = request.Headers |> appendHeaderNoRepeat header}

/// Adds an HTTP Basic Authentication header, which includes the username and password encoded as a base-64 string
let withBasicAuthentication username password (request:Request) =
    let authHeader = Authorization ("Basic " + Convert.ToBase64String(Encoding.GetEncoding(ISO_Latin_1).GetBytes(username + ":" + password)))
    {request with Headers = request.Headers |> appendHeaderNoRepeat authHeader}

/// Sets the the request body, using ISO Latin 1 character encoding.
///
/// Only certain request types should have a body, e.g. Posts.
let withBody body request =
    {request with Body = Some(body); BodyCharacterEncoding = Some(ISO_Latin_1)}

    /// Sets the the request body, using ISO Latin 1 character encoding.
///
/// Only certain request types should have a body, e.g. Posts.
let withBodyAsBytes body request =
    {request with BodyBytes = Some(body)}

/// Sets the request body, using the provided character encoding.
let withBodyEncoded body characterEncoding request =
    {request with Body = Some(body); BodyCharacterEncoding = Some(characterEncoding)}

/// Adds the provided QueryString record onto the request URL.
/// Multiple items can be appended.
let withQueryStringItem item request =
    {request with QueryStringItems = request.QueryStringItems |> append item}

/// Adds all the provided QueryStrings record onto the request URL at once
/// Multiple items can be appended.
let withQueryStringItems items request =
    {request with QueryStringItems = items}

/// Adds a cookie to the request
/// The domain will be taken from the URL, and the path set to '/'.
///
/// If your cookie appears not to be getting set, it could be because the response is a redirect,
/// which (by default) will be followed automatically, but cookies will not be re-sent.
let withCookie cookie request =
    if not request.CookiesEnabled then failwithf "Cannot add cookie %A - cookies disabled" cookie.name
    {request with Cookies = request.Cookies |> append cookie}

/// Decodes the response using the specified encoding, regardless of what the response specifies.
///
/// If this is not set, response character encoding will be:
///  - taken from the response content-encoding header, if provided, otherwise
///  - ISO Latin 1
///
/// Many web pages define the character encoding in the HTML. This will not be used.
let withResponseCharacterEncoding encoding request:Request = 
    {request with ResponseCharacterEncoding = Some(encoding)}
    
/// Sets the keep-alive header.  Defaults to true.
///
/// If true, Connection header also set to 'Keep-Alive'
/// If false, Connection header also set to 'Close'
///
/// NOTE: If true, headers only sent on first request.
let withKeepAlive value request =
    {request with KeepAlive = value}

let private getMethodAsString request =
    match request.Method with
        | Options -> "Options"
        | Get -> "Get"
        | Head -> "HEAD"
        | Post -> "POST"
        | Put -> "PUT"
        | Delete -> "DELETE"
        | Trace -> "TRACE"
        //| Connect -> "CONNECT"

let private getMethodAsHttpRequest request =
    match request.Method with
        | Options -> System.Net.Http.HttpMethod.Options
        | Get -> System.Net.Http.HttpMethod.Get
        | Head -> System.Net.Http.HttpMethod.Head
        | Post -> System.Net.Http.HttpMethod.Post
        | Put -> System.Net.Http.HttpMethod.Put
        | Delete -> System.Net.Http.HttpMethod.Delete
        | Trace -> System.Net.Http.HttpMethod.Trace
        //| Connect -> System.Net.Http.HttpMethod.

let private getQueryString request = 
    match request.QueryStringItems.IsSome with
    | true -> request.QueryStringItems.Value 
                |> List.fold (
                    fun currentQueryString queryStringItem -> 
                        (if currentQueryString = "?" then currentQueryString else currentQueryString + "&" ) 
                        + urlEncode(queryStringItem.name)
                        + "=" 
                        + urlEncode(queryStringItem.value)) 
                    "?"
    | false -> ""

// Sets headers on HttpNetClient.
// Mutates HttpNetClient.
let private setHeaders (headers:RequestHeader list option) (webRequest:System.Net.Http.HttpClient) =
    if headers.IsSome then
        headers.Value
        |> List.iter (fun header ->
            match header with
            | Accept(value) -> webRequest.DefaultRequestHeaders.Accept.Add(new Headers.MediaTypeWithQualityHeaderValue(value))
            | AcceptCharset(value) -> webRequest.DefaultRequestHeaders.Add("Accept-Charset", value)
            | AcceptDatetime(value) -> webRequest.DefaultRequestHeaders.Add("Accept-Datetime", value)
            | AcceptLanguage(value) -> webRequest.DefaultRequestHeaders.Add("Accept-Language", value)
            | Authorization(value) -> webRequest.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", value) |> ignore
            | RequestHeader.Connection(value) -> () //webRequest.Connection <- value
            | RequestHeader.ContentMD5(value) -> webRequest.DefaultRequestHeaders.Add("Content-MD5", value)
            | RequestHeader.ContentType(value) -> 
                webRequest.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type",value) |> ignore
            | RequestHeader.Date(value) -> () //webRequest.Date <- value
            | Expect(value) -> () //webRequest.Expect <- value.ToString()
            | From(value) -> webRequest.DefaultRequestHeaders.Add("From", value)
            | IfMatch(value) -> webRequest.DefaultRequestHeaders.Add("If-Match", value)
            | IfModifiedSince(value) -> () //webRequest.IfModifiedSince <- value
            | IfNoneMatch(value) -> webRequest.DefaultRequestHeaders.Add("If-None-Match", value)
            | IfRange(value) -> webRequest.DefaultRequestHeaders.Add("If-Range", value)
            | MaxForwards(value) -> webRequest.DefaultRequestHeaders.Add("Max-Forwards", value.ToString())
            | Origin(value) -> webRequest.DefaultRequestHeaders.Add("Origin", value)
            | RequestHeader.Pragma(value) -> webRequest.DefaultRequestHeaders.Add("Pragma", value)
            | ProxyAuthorization(value) -> webRequest.DefaultRequestHeaders.Add("Proxy-Authorization", value)
            | Range(value) -> () //webRequest.AddRange(value.start, value.finish)
            | Referer(value) -> () //webRequest.Referer <- value
            | Upgrade(value) -> webRequest.DefaultRequestHeaders.Add("Upgrade", value)
            | UserAgent(value) -> () //webRequest.UserAgent <- value
            | RequestHeader.Via(value) -> webRequest.DefaultRequestHeaders.Add("Via", value)
            | RequestHeader.Warning(value) -> webRequest.DefaultRequestHeaders.Add("Warning", value)
            | Custom( {name=customName; value=customValue}) -> webRequest.DefaultRequestHeaders.Add(customName, customValue))

let private setHeadersToClient (headers:RequestHeader list option) (content:System.Net.Http.StringContent) =
    if headers.IsSome then
        headers.Value
        |> List.iter (fun header ->
            match header with
            | RequestHeader.ContentType(value) -> 
                content.Headers.ContentType <- new Headers.MediaTypeWithQualityHeaderValue(value)
            | _ -> ())
            

// Sets body 
let setBody (body:string option) (encoding:string option) =
    if body.IsSome then
        if encoding.IsNone then
            failwith "Body Character Encoding not set"
        let bodyBytes = Encoding.GetEncoding(encoding.Value).GetBytes(body.Value)
        // Getting the request stream seems to be actually connecting to the internet in some way
        use requestStream = new MemoryStream()
        requestStream.AsyncWrite(bodyBytes, 0, bodyBytes.Length) |> Async.RunSynchronously

// The nasty business of turning a Request into an HttpNetClient
let private toHttpNetClient request =
    let requestMsg = new HttpRequestMessage()
    requestMsg

// Get the header as a ResponseHeader option.  Is an option because there are some headers we don't want to set.
let private getResponseHeader headerName =
    match headerName with
    | null -> None
    | "Access-Control-Allow-Origin" -> Some(AccessControlAllowOrigin)
    | "Accept-Ranges" -> Some(AcceptRanges)
    | "Age" -> Some(Age)
    | "Allow" -> Some(Allow)
    | "Cache-Control" -> Some(CacheControl)
    | "Connection" -> Some(ResponseHeader.Connection)
    | "Content-Encoding" -> Some(ContentEncoding)
    | "Content-Language" -> Some(ContentLanguage)
    | "Content-Length" -> None
    | "Content-Location" -> Some(ContentLocation)
    | "Content-MD5" -> Some(ResponseHeader.ContentMD5Response)
    | "Content-Disposition"| "ContentDisposition"| "content-disposition"| "contentdisposition" -> Some(ContentDisposition)
    | "Content-Range" -> Some(ContentRange)
    | "ContentType" | "Content-Type" | "content-type"-> Some(ResponseHeader.ContentTypeResponse)
    | "Date" -> Some(ResponseHeader.DateResponse)
    | "ETag" -> Some(ETag)
    | "Expires" -> Some(Expires)
    | "Last-Modified" -> Some(LastModified)
    | "Link" -> Some(Link)
    | "Location" -> Some(Location)
    | "P3P" -> Some(P3P)
    | "Pragma" -> Some(ResponseHeader.PragmaResponse)
    | "Proxy-Authenticate" -> Some(ProxyAuthenticate)
    | "Refresh" -> Some(Refresh)
    | "Retry-After" -> Some(RetryAfter)
    | "Server" -> Some(Server)
    | "Set-Cookie" -> None
    | "Strict-Transport-Security" -> Some(StrictTransportSecurity)
    | "Trailer" -> Some(Trailer)
    | "Transfer-Encoding" -> Some(TransferEncoding)
    | "Vary" -> Some(Vary)
    | "Via" -> Some(ResponseHeader.ViaResponse)
    | "Warning" -> Some(ResponseHeader.WarningResponse)
    | "WWW-Authenticate" -> Some(WWWAuthenticate)
    | _ -> Some(NonStandard headerName)

// Gets the headers from the passed response as a map of ResponseHeader and string.
let private getHeadersAsMap (response:HttpResponseMessage) =
    // TODO: Find a better way of dong this
    let mutable index = -1
    let headerArray = Array.zeroCreate 255
    let enumerator = response.Content.Headers.GetEnumerator()
    while enumerator.MoveNext() do
        let h = enumerator.Current
        index <- index + 1
        headerArray.[index] <- 
            match getResponseHeader h.Key with
            | Some(headerKey) -> Some((headerKey, List.ofSeq h.Value))
            | None -> None
    headerArray
    |> Array.filter (fun item -> item <> None)
    |> Array.map Option.get
    |> Map.ofArray

let private mapEncoding (encoding:string) =
    match responseEncodingMappings.TryFind(encoding.ToLower()) with
        | Some(mappedEncoding) -> mappedEncoding
        | None -> encoding

let private readBody encoding (response:HttpResponseMessage) = async {
    let charset = 
        match encoding with
        | None -> Encoding.GetEncoding(ISO_Latin_1)
        | Some(enc) -> 
            match getEncodingPage enc with
            | Some (e) -> Encoding.GetEncoding(mapEncoding e:string)
            | None -> Encoding.GetEncoding(ISO_Latin_1)
    
    use responseStream = new StreamReader(response.Content.ReadAsStreamAsync().Result,charset)
    let body = responseStream.ReadToEnd()
    return body
}

// Uses the HttpNetClient to get the response.
// HttpNetClient throws an exception on anything but a 200-level response,
// so we handle such exceptions and return the response.
let private getResponseNoException (request:Request) = async {
    let url = request.Url + (request |> getQueryString)
    let handler = new ModernHttpClient.NativeMessageHandler()
    let httpClient = new HttpClient(handler)

    httpClient.DefaultRequestHeaders.Add(XKidoSDK, XKidoSDKValue);
    httpClient.DefaultRequestHeaders.Add(XKidoSDKVersion, XKidoSDKVersionValue);

    httpClient |> setHeaders request.Headers
    let requestMsg = new HttpRequestMessage(request |> getMethodAsHttpRequest, url)

    match request.Body with
    | Some (b) -> 
        let content = new StringContent(b)
        content |> setHeadersToClient request.Headers
        requestMsg.Content <- content
    | None -> () 

    //TODO: make this generic to use in other cases, not only for KidoZen
    match request.BodyBytes with
        | Some bytes ->
            let xfilenameheader = Custom( {name="x-file-name"; value=""}) 
            let fileContent =  new StreamContent(new MemoryStream(bytes))
            fileContent.Headers.ContentType <- new Headers.MediaTypeHeaderValue("application/octet-stream"); 
            match request.Headers with
                | Some h ->  
                    match h |> headerGet xfilenameheader with
                        | Some xfile -> 
                            match xfile with
                                | Custom c -> fileContent.Headers.Add("x-file-name", c.value)
                                | _ -> failwith "Header 'x-file-name' must be a custom header" 
                        | _ -> failwith "Header 'x-file-name' must exists" 
                | None -> failwith "Header 'x-file-name' must exists" 
            requestMsg.Content <- fileContent
        | None -> ()

    return httpClient.SendAsync(requestMsg)
}

/// Sends the HTTP request and returns the response code as an integer, asynchronously.
let getResponseCodeAsync (request:Request) = async {
    let! response = request |> getResponseNoException 
    return response.Result.StatusCode |> int
}

/// Sends the HTTP request and returns the response code as an integer.
let getResponseCode request =
    getResponseCodeAsync request |> Async.RunSynchronously

/// Sends the HTTP request and returns the response body as a string, asynchronously.
///
/// Gives an empty string if there's no response body.
let getResponseBodyAsync request = async {
    let! response = request |> getResponseNoException 
    let! body = response.Result |> readBody request.ResponseCharacterEncoding
    return body
}

/// Sends the HTTP request and returns the response body as a string.
///
/// Gives an empty string if there's no response body.
let getResponseBody request =
    getResponseBodyAsync request |> Async.RunSynchronously

/// Sends the HTTP request and returns the full response as a Response record, asynchronously.
let getResponseAsync request = async {
    let! response = request |> getResponseNoException 

    let code = response.Result.StatusCode |> int
    let lenght = response.Result.Content.Headers.ContentLength
    let headers = response.Result |> getHeadersAsMap

    let encoding = 
        match code with
        | 204 -> Some UTF_8
        | _ ->
            match request.ResponseCharacterEncoding with
                | Some rce -> Some rce
                | None -> 
                    let ctl = List.ofSeq headers.[ContentTypeResponse]
                    Some ctl.Head

    let! body = response.Result|> readBody  encoding
    
    let entityBody = 
        match body.Length > 0 with
        | true -> Some(body)
        | false -> None

    return {   
        StatusCode = code
        BytesBody = None
        EntityBody = entityBody
        ContentLength = lenght.GetValueOrDefault()
        //TODO: support when we need to support cookies
        Cookies = Some (Map.ofList [ ("1", "one"); ("2", "two"); ("3", "three") ]) 
        Headers = headers
    }
}

/// Sends the HTTP request and returns the full response as a Response record, asynchronously.
let getResponseBytesAsync request = async {
    let! response = request |> getResponseNoException 

    let code = response.Result.StatusCode |> int
    let lenght = response.Result.Content.Headers.ContentLength
    let headers = response.Result |> getHeadersAsMap

    let encoding = 
        match code with
        | 204 -> Some UTF_8
        | _ ->
            match request.ResponseCharacterEncoding with
                | Some rce -> Some rce
                | None -> 
                    let ctl = List.ofSeq headers.[ContentTypeResponse]
                    Some ctl.Head

    let bodybytes = response.Result.Content.ReadAsByteArrayAsync().Result
    
    let bytes = 
        match true with
        | true -> Some(bodybytes)
        | false -> None

    let! body = response.Result|> readBody  encoding
    let entityBody = 
        match code with
        | 200 -> None
        | _ -> Some(body)

    return {   
        StatusCode = code
        BytesBody = bytes
        EntityBody = entityBody
        ContentLength = lenght.GetValueOrDefault()
        Cookies = Some (Map.ofList [ ("1", "one"); ("2", "two"); ("3", "three") ])
        Headers = headers
    }
}



/// Sends the HTTP request and returns the full response as a Response record.
let getResponse request =
    getResponseAsync request |> Async.RunSynchronously

