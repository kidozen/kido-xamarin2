using Xamarin.Forms;
#if __IOS__
    using Kidozen.iOS;
#else
    using Kidozen.Android;
#endif
using Sync.Data;
using Sync.Model;
namespace Sync
{
    public class App : Application
    {
        private static App thisApplication;
        public static TodoItemsSource TodoItemsSource =  new Model.TodoItemsSource( KidozenTodoItemDataSync.GetInstance() );

        public App()
        {
            MainPage = new NavigationPage(new LoginPageView()); 
            if (thisApplication == null)
            {
                App.thisApplication = this;
            }
        }

        internal static Page GetLoginPage()
        {
            return thisApplication.MainPage;
        }

        internal static Page GetMainListPage()
        {
            return new NavigationPage(new MainListPage() );

        }
    }
}

