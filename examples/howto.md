# kido.dll (pcl core library)
The SDK uses JSON.net for serialization & deserialization. You must add a reference to this component via Xamarin Component Store
  - Browse to release/core add a reference to kido.dll
  - Browse to release/support add a reference to ModernHttpClient.dll


# Platform specific

iOS and Android uses SQLite for offline cache. You must add a reference to this component via Xamarin Component

## For MonoTouch

For iOS that requires Passive, Crash, DataVisualization or Offline:
  - Browse to release/core add a reference to kido.dll
  - Browse to release/support add a reference to ModernHttpClient.dll
  - Browse to release/ios add a reference to kido.ios.dll
  - SharpZip Lib from nugget

## For MonoDroid

For Android that requires Passive, Crash, DataVisualization or Offline:
  - Browse to release/core add a reference to kido.dll
  - Browse to release/support add a reference to ModernHttpClient.dll
  - Browse to release/android add a reference to kido.android.dll
  - You must also add a reference to Mono.Android.Export.dll
  
