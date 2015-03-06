using System;
using System.Drawing;
using System.Collections.Generic;
using Foundation;
using UIKit;

using Kidozen.iOS;

namespace kido.ui.tests
{
    public partial class DetailViewController : UIViewController
    {
        object testExpectedDetail;

        public DetailViewController(IntPtr handle)
            : base(handle)
        {
        }

        public void SetDetailItem(object newDetailItem)
        {
            if (testExpectedDetail != newDetailItem)
            {
                testExpectedDetail = newDetailItem;
            }
        }


        void ShowExpectedResult()
        {
            var kidozen = new Kidozen.KidoApplication("kidodemo.kidocloud.com", "tasks"
                , "wb8KTX2/21A6ISM7PncaNozhxxCxcL8+TtB2aKbZyu8=");
            kidozen.Authenticate("demo@kidozen.com","pass","Kidozen").ContinueWith(authTask => 
                {
                    try 
	                    {
                            var ps = kidozen.SubscribeToChannel("test");
                            var ok = ps.Subscribe().Result;
                            ps.Publish(new { bar = "foo" });                           
	                    }
	                    catch (Exception e)
	                    {
		                    throw;
	                    }
                //
                //InvokeOnMainThread(()=> {
                    //assert
                    //var expected = testExpectedDetail as KidoUIXTestDetails<string>;
                    //var isOk = expected.ExpectedValue == kidozen.CurrentUser.UserName;
                    //detailDescriptionLabel.Text = kidozen.CurrentUser.UserName;
                //    })
                });
 
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ShowExpectedResult();
        }
    }
}
