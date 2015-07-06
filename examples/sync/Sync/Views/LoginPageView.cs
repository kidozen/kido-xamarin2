using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Kidozen;
using System.Threading.Tasks;
using Sync.Data;


#if __IOS__
using Kidozen.iOS;
#else
using Kidozen.Android;

#endif

namespace Sync
{
    public class LoginPageView : ContentPage
    {
        Button loginButton;
        public LoginPageView()
        {
            NavigationPage.SetHasNavigationBar(this, true);
            
            loginButton = new Button
            {
                Text = "Login"
            };
            loginButton.Clicked += (o, s) => {
				(App.TodoItemsSource.Current as KidozenTodoItemDataSync).NoAuthenticate();
				//Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(App.GetMainListPage()));

				App.TodoItemsSource.Current
					.Authenticate(); Navigate();
					////.AsyncAuthenticate()
					////.ContinueWith(t=>Navigate());
                };

            
            var layout = new StackLayout();
            layout.Children.Add(loginButton);
            layout.VerticalOptions = LayoutOptions.FillAndExpand;
            Content = layout;

        }

        private void Navigate()
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(App.GetMainListPage()));
        }


    }
}