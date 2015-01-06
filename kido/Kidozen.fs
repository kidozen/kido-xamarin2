namespace Kidozen

open System
open System.Net
open System.Threading.Tasks
open System.Runtime.InteropServices
open KzApplication

type User(name,rawtoken) =
    member this.Name = name
    member this.RawToken = rawtoken

type PassiveAuthenticationEventArgs(token:string) =
    inherit System.EventArgs()
    member this.Token = token

type KidoApplication(marketplace, application, key )  =
    let zeroIdentity = { token= None; rawToken = ""; id = ""; config = ""; expiration = System.DateTime.Now; authenticationRequest  = { Key = ""; ProviderRequest = None; Marketplace = marketplace; Application = application  } }
    let mutable identity = zeroIdentity
    let mutable currentUser = User("","")
    let onPassiveAuth = new DelegateEvent<System.EventHandler<PassiveAuthenticationEventArgs>>()
    member this.setUser user = currentUser <- user
    member this.marketplace = marketplace
    member this.application = application
    member this.key = key
    member this.setIdentity id = identity <- id
    member this.GetIdentity = identity
    member this.GetPassiveDelegate = onPassiveAuth

    [<CLIEvent>]
    member this.OnPassiveAuthenticationComplete = onPassiveAuth.Publish

    member private this.authWithUser user password provider = async {
        let! result = asyncGetFederatedToken marketplace application user password provider
        match result with
        | Token t -> 
            identity <- t
            currentUser <- User(user,t.rawToken)
            return currentUser
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
