// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace DataVisTouch
{
	[Register ("DataVisTouchViewController")]
	partial class DataVisTouchViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton authButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField editTextName { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton invokeButton { get; set; }

		[Action ("authButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void authButton_TouchUpInside (UIButton sender);

		[Action ("invokeButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void invokeButton_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (authButton != null) {
				authButton.Dispose ();
				authButton = null;
			}
			if (editTextName != null) {
				editTextName.Dispose ();
				editTextName = null;
			}
			if (invokeButton != null) {
				invokeButton.Dispose ();
				invokeButton = null;
			}
		}
	}
}
