The SDK uses JSON.net for serialization & deserialization. You must add a reference to this component via Xamarin Component Store ( nuget, or the .dll file)
- - 

iOS and Android uses SQLite for offline cache. You must add a reference to this component via Nuget ( Tamarin Component, or the .dll file)

Browse to release/core add a reference to kido.dll
Browse to release/support add a reference to ModernHttpClient.dll

For iOS that requires Passive, Crash, DataVisualization or Offline:
Browse to release/ios add a reference to kido.ios.dll
SharpZip Lib from nugget

For Android that requires Passive, Crash, DataVisualization or Offline:
Browse to release/android add a reference to kido.android.dll
Also add a reference to
Error XA4210: You need to add a reference to Mono.Android.Export.dll when you use ExportAttribute or ExportFieldAttribute. (XA4210) (DataVisDroid)
