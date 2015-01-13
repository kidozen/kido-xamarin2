using System;
using System.Collections.Generic;
using System.Linq;

#if __UNIFIED__
using MonoTouch;
using UIKit;
using Foundation;
#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Xamarin.Forms;
#endif


namespace Todo.iOSClassic
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		
		public override UIWindow Window {
			get;
			set;
		}
		
		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Forms.Init ();
			Window.RootViewController = App.GetLoginPage ().CreateViewController ();
			return true;

		}

	}
}

