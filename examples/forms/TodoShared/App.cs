using System;
using Xamarin.Forms;
#if __ANDROID__
using Android.Content;
#endif
namespace Todo
{
	public class App: Application
	{		
		#if __ANDROID__
		public static Context AndroidContext { get; set;}
		#endif

		public App() {
			MainPage = new ContentPage ();
		}

		public static Page GetLoginPage ()
		{
			database = new TodoItemDatabase();

			var loginNav = new NavigationPage (new LoginPage ());
			return loginNav;
		}

		public static Page GetMainPage ()
		{
			var mainNav = new NavigationPage (new TodoListPage ());
			return mainNav;
		}

		static TodoItemDatabase database;
		public static TodoItemDatabase Database {
			get { 
				if (database == null) {
					database = new TodoItemDatabase ();
				}
				return database; 
			}
		}
	}
}

