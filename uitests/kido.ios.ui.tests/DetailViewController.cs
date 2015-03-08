using System;
using System.Drawing;
using System.Collections.Generic;
using Foundation;
using UIKit;

using Kidozen.iOS;

namespace kido.ui.tests
{
    public class PSMessage  {
        public string bar { get; set; }
    }

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
            kidozen.Authenticate().ContinueWith(authTask => 
                {
                    try 
	                    {
                            var ps = kidozen.SubscribeToChannel<PSMessage>("ABCDEF-0000000", onMessageArrive);
                            var ok = ps.Subscribe().ContinueWith(
                                t => {
                                    Console.WriteLine("Task Subscribe Result: " + t.Result.ToString());
                                    ps.Publish(new PSMessage { bar = "2" });
                                }    
                            );
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

        private void onMessageArrive(object instance, EventArgs value)
        {
            throw new NotImplementedException();
        }

        void ps_MyEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
