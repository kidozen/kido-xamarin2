# KidoZen .NET SDK 2.0
This is a new version of the SDK for .NET platform Kidozen 
## Version
2.0.1.0
## Whats new?

- iOS 64b support
- Crash Reports
- DataVisualization
- Passive Authentication
- DataSources and Services allows to download files

From the technical point of view provides a library core 'kido.dll' for creating applications that make use of the following basic services without being recompiled for each platform: 
- Active Authentication 
- Storage 
- Queue 
- DataSources 
- Enterprise API services 
- Configuration 
- SMS 
- EMail

Supported platforms
  - iOS
  - Android
  - Windows Phone 8.1
  - Windows RT

## How to install
 - Install from NuGet gallery https://www.nuget.org. 
 >The package is marked as pre-reolease, to get it please refer to the following documentation https://nuget.codeplex.com/wikipage?title=Pre-Release%20Packages
 - ... ot clone the sources and add the project references to your application

## TODO
  - Add support to Passive Auth, Crash and DataVisualization to Windows
  - Add PubSub service support
  - Documentation
  - More Samples

## Getting started with the code
### Creating a new Application Object
```
using Kidozen;
var kido = new KidoApplication("contoso.kidocloud.com", "app_name", "SDK Key");
```
### Authentication
```
    Boolean isAuthenticated = false;
    kido.Authenticate("you@kidozen.com", "supersecret", "Kidozen")
        .ContinueWith(t => { isAuthenticated = ! t.IsFaulted;}
    );

```
> More information abuot KidoZen [Security](http://docs.kidozen.com/security-gateway/)

### Using KidoZen services ej: DataSource
```
    var qds = kido.DataSource("GetWeather");
    qds.Query<JObject>(new { city = "London"}).ContinueWith(d => {
        Console.WriteLine(d.Result);
        }
    );
```
> More information about [DataSources](http://docs.kidozen.com/data-sources/)


### Passive Authentication

```
    Boolean isAuthenticated = false;
    kido.Authenticate().ContinueWith(t => { isAuthenticated = ! t.IsFaulted;});
```
> This version of the SDK only supports passive authentication for iOS and Android

## Technical 

The SDK uses a number of open source projects to work properly:

* JSON.net http://james.newtonking.com/json
* SQLite - http://www.sqlite.org/

> Prerelease Version.
> The following features are present but consider it as 'Beta' version in iOS and Android: 
> Analitics, Crash


## Known issues
  - Passive authentication and Data visualization: does not display a progress message )

License
----

MIT

