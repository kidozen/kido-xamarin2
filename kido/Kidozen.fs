namespace Kidozen

open System
open System.Net
open System.Collections
open System.Collections.Generic
open System.Threading.Tasks
open System.Runtime.InteropServices
open System.Runtime.CompilerServices

open KzApplication
open Utilities

[<assembly:InternalsVisibleTo("kido.ios")>]
[<assembly:InternalsVisibleTo("kido.android")>]
do()

type IAnalytics =
    abstract member Enable : Boolean -> Unit

type User (rawtoken:string) =
    let mutable token = rawtoken
    member this.RawToken = token
    new() = User(String.Empty)
    member this.AllClaims = keyValueStringToDictionary token
    member this.UserName = this.AllClaims.Value.["http://schemas.kidozen.com/name"] 

type PassiveAuthenticationEventArgs(token:string) =
    inherit System.EventArgs()
    member this.Token = token


type KidoApplication(marketplace, application, key )  =
    let zeroIdentity = { token= None; rawToken = ""; id = ""; config = ""; expiration = System.DateTime.Now; authenticationRequest  = { Key = ""; ProviderRequest = None; Marketplace = marketplace; Application = application  } }
    let mutable identity = zeroIdentity
    let mutable authenticated = false
    // TODO: validate what must be internal
    member this.marketplace = marketplace
    member this.application = application
    member this.key = key
    member this.setIdentity id = identity <- id
    member this.GetIdentity = identity
    
    member this.IsAuthenticated = authenticated
    member val CurrentUser = new User()  with get,set

    //passive authentication support
    member this.setPassiveIdentity tokenRaw tokenRefresh =
        //TODO : Meomoize
        let cfg =  match getAppConfig (createConfigUrl this.marketplace this.application) with
                    | Configuration c -> c 
                    | _ ->  raise (System.ArgumentException("fail getting app configuration once again"))
        this.CurrentUser <- User(tokenRaw)
        let tokenExpiresOn = getExpiration tokenRaw
        let tk =  { raw = Some( tokenRaw ) ; refresh = Some (tokenRefresh) ; expiration = Some( tokenExpiresOn.ToString()) };
        let passiveIdentity = {
            id = this.CurrentUser.AllClaims.Value.["http://schemas.kidozen.com/userid"];
            rawToken = tokenRaw;
            token =  Some (tk);
            config = cfg;
            expiration = tokenExpiresOn;
            authenticationRequest  = { Key = key; ProviderRequest = None; Marketplace = marketplace; Application = application  }
        }
        identity<-passiveIdentity
        authenticated <- true

    member private this.authWithUser user password provider = async {
        let! result = asyncGetFederatedToken marketplace application user password provider
        match result with
        | Token t -> 
            identity <- t
            authenticated <- true
            this.CurrentUser <- User(t.rawToken)
            return this.CurrentUser
        | InvalidApplication e -> return raise e
        | InvalidCredentials e -> return raise e
        | InvalidIpCredentials e -> return raise e
    }

    member this.Log = 
        new Log(identity)

    member this.MailSender = 
        new MailSender(identity)

    member this.Files = 
        new Files(identity)

    member this.Authenticate (user, password, provider)= 
        this.authWithUser user password provider |> Async.StartAsTask 

    member this.DataSource name = 
        new DataSource(name, identity)
    
    member this.ObjectSet name = 
        new ObjectSet(name, identity)

    member this.Queue name = 
        new Queue(name,identity)

    member this.Service name =
        new Service(name,identity)

    member this.Configuration name = 
        new Configuration(name, identity)

    member this.Sms number = 
        new Sms(number, identity)

    member this.WriteLog message data level =
        this.Log.Create(message,data, level, this.marketplace, this.application, this.key)

    member this.QueryLog query =
        this.Log.Query(query)

    member this.QueryLog<'a>query =
        this.Log.Query<'a>(query)

    member this.AllLog =
        this.Log.Query("{\"query\":{\"match_all\":{}}}")

    member this.ClearLog =
        this.Log.Clear()

    //helpers to JS injection
    member internal this.getPassiveAuthInformation =
        let d = new Dictionary<_, _>()
        d.["refresh_token"] <- identity.token.Value.refresh.Value
        d.["rawToken"] <- identity.token.Value.raw.Value
        d :> IDictionary<String, String>

    member internal this.getActiveAuthInformation =
        let d = new Dictionary<_, _>()
        d.["username"] <- identity.authenticationRequest.ProviderRequest.Value.User
        d.["password"] <- identity.authenticationRequest.ProviderRequest.Value.Password
        d.["provider"] <- identity.authenticationRequest.ProviderRequest.Value.Key
        d :> IDictionary<String, String>

    member internal this.isPassiveAuthenticated =
        identity.authenticationRequest.ProviderRequest.IsSome
    