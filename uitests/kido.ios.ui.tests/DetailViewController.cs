using System;
using System.Drawing;
using System.Collections.Generic;
using Foundation;
using UIKit;

using Kidozen;
using Kidozen.iOS;

namespace kido.ui.tests
{
    public class PSMessage  {
        public string bar { get; set; }
    }

    public partial class DetailViewController : UIViewController
    {
        object testExpectedDetail;
        Kidozen.PubSub ps;
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
                            ps = kidozen.SubscribeToChannel<PSMessage>("ABCDEF-000010", onMessageArrive);
                            var ok = ps.Subscribe().ContinueWith(
                                t => {
                                    Console.WriteLine("Task Subscribe Result: " + t.Result.ToString());
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

        private void onMessageArrive(object obj, EventArgs value)
        {
            Console.WriteLine("Message arrived: " + value.ToString());
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

        partial void UIButton7_TouchUpInside(UIButton sender)
        {
            var message = new PSMessage { bar = Guid.NewGuid().ToString() };
            var result = ps.Publish(message).Result;
            Console.WriteLine("Publish result: " + result);
        }
    }
}
