// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace AnalyticsTouch
{
	[Register ("RootViewController")]
	partial class RootViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton SignInButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton SignOutButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton TagClickButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton TagCustomButton { get; set; }

		[Action ("SignInButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void SignInButton_TouchUpInside (UIButton sender);

		[Action ("SignOutButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void SignOutButton_TouchUpInside (UIButton sender);

		[Action ("TagClickButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void TagClickButton_TouchUpInside (UIButton sender);

		[Action ("TagCustomButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void TagCustomButton_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (SignInButton != null) {
				SignInButton.Dispose ();
				SignInButton = null;
			}
			if (SignOutButton != null) {
				SignOutButton.Dispose ();
				SignOutButton = null;
			}
			if (TagClickButton != null) {
				TagClickButton.Dispose ();
				TagClickButton = null;
			}
			if (TagCustomButton != null) {
				TagCustomButton.Dispose ();
				TagCustomButton = null;
			}
		}
	}
}
