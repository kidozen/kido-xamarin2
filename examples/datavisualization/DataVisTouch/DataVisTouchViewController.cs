using System;
using CoreGraphics;

using System.Diagnostics;
using Foundation;
using UIKit;

using Examples;

namespace DataVisTouch
{
	public partial class DataVisTouchViewController : UIViewController
	{
		Model kidoModel = new Model();

		public DataVisTouchViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
		}

		#region View lifecycle

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();


		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion

		partial void authButton_TouchUpInside (UIButton sender)
		{
			kidoModel.Authenticate ().ContinueWith (t =>
				InvokeOnMainThread (() => {
					invokeButton.Enabled = t.Result;
					editTextName.Enabled = t.Result; 
				} )
			);
		}

		partial void invokeButton_TouchUpInside (UIButton sender)
		{
			Debug.WriteLine("Launching visualization, please wait");
			kidoModel.DisplayDataVisualization(editTextName.Text)
				.ContinueWith (t=> {
					Debug.WriteLine("visualization loading, please wait");
				} );
		}
	}
}

