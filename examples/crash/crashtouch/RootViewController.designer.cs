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

namespace crashtouch
{
	[Register ("RootViewController")]
	partial class RootViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton authButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton outOFIndexButton { get; set; }

		[Action ("authButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void authButton_TouchUpInside (UIButton sender);

		[Action ("outOFIndexButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void outOFIndexButton_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (authButton != null) {
				authButton.Dispose ();
				authButton = null;
			}
			if (outOFIndexButton != null) {
				outOFIndexButton.Dispose ();
				outOFIndexButton = null;
			}
		}
	}
}
