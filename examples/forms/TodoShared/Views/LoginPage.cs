using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;

#if __IOS__
using Kidozen.iOS;
#else
using Kidozen.Android;
using Android.Content;
#endif

namespace Todo
{
	public class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			Title = "Todo";

			NavigationPage.SetHasNavigationBar (this, true);

			var loginButton = new Button { Text = "LogIn" };
			loginButton.Clicked += (sender, e) => {
				App.Database.Login().ContinueWith(
					t => {
					if (t.Result) {
							Xamarin.Forms.Device.BeginInvokeOnMainThread( () => Navigation.PushModalAsync(App.GetMainPage()));
						}
					}
				);

			};

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.StartAndExpand,
				Padding = new Thickness(20),
				Children = {
					loginButton
				}
			};
		}

	}
}

