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

namespace MonoTouchDs
{
	[Register ("MonoTouchDsViewController")]
	partial class MonoTouchDsViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnAuthenticate { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnQuery { get; set; }

		[Action ("btnAuthenticate_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void btnAuthenticate_TouchUpInside (UIButton sender);

		[Action ("btnQuery_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void btnQuery_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (btnAuthenticate != null) {
				btnAuthenticate.Dispose ();
				btnAuthenticate = null;
			}
			if (btnQuery != null) {
				btnQuery.Dispose ();
				btnQuery = null;
			}
		}
	}
}
