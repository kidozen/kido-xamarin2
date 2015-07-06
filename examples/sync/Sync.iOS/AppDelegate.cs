using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Kidozen.iOS;
using Xamarin.Forms;
using Sync.Data;

namespace Sync.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public ITodoItemRepository ItemsRepository
        {
            get;
            set;
        }

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new Sync.App());
  
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            Window.RootViewController = App.GetLoginPage().CreateViewController();
            Window.MakeKeyAndVisible();

            return true;
        }
    }
}
