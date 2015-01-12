using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using MonoTouch;
using Foundation;
using UIKit;

using Xamarin.Forms;
using Todo;
using Kidozen.iOS;

namespace Passive
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override UIWindow Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			global::Xamarin.Forms.Forms.Init ();
			LoadApplication (new App ()); 
			Window.RootViewController = App.GetLoginPage ().CreateViewController ();
			return base.FinishedLaunching (application, launchOptions);

		}


	}
}