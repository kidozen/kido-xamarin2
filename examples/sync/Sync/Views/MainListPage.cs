#if __IOS__
using Kidozen.iOS;
#else
using Kidozen.Android;
#endif

using System;
using System.Timers;
using Xamarin.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Sync
{
    public class MainListPage : ContentPage
    {
        internal readonly ListView ListView;

        public MainListPage()
        {

            Title = "Sync";

            NavigationPage.SetHasNavigationBar(this, true);

            ListView = new ListView
            {
                RowHeight = 40,
                ItemTemplate = new DataTemplate(typeof(SyncItemCell))
            };
            // These commented-out lines were used to test the ListView prior to integrating the database

            ListView.ItemSelected += (sender, e) =>
            {
                var todoItem = (TodoItem)e.SelectedItem;
                var todoPage = new SyncItemPage { BindingContext = todoItem };
                Navigation.PushAsync(todoPage);
            };

            var layout = new StackLayout();
            layout.Children.Add(ListView);
            layout.VerticalOptions = LayoutOptions.FillAndExpand;
            

            ToolbarItem addNewItemButton = null;
            ToolbarItem synchronizeButton = null;

            if (Device.OS == TargetPlatform.Android)
            { // BUG: Android doesn't support the icon being null
                addNewItemButton = new ToolbarItem("+", "plus", () =>
                {
                    var todoItem = new TodoItem();
                    var todoPage = new SyncItemPage { BindingContext = todoItem };
                    Navigation.PushAsync(todoPage);
                });
                synchronizeButton = new ToolbarItem("Sync", "plus", () =>
                {
                    App.TodoItemsSource.Current.Synchronize();
                });
            }

            if (Device.OS == TargetPlatform.iOS)
            {
                addNewItemButton = new ToolbarItem("+", null, () =>
                    {
                        var todoItem = new TodoItem();
                        var todoPage = new SyncItemPage { BindingContext = todoItem };
                        Navigation.PushAsync(todoPage);
                    });

                synchronizeButton = new ToolbarItem("Sync", null, () =>
                {
                    App.TodoItemsSource.Current.Synchronize();
                });
            }

            if (addNewItemButton != null) ToolbarItems.Add(addNewItemButton);
            if (synchronizeButton != null) ToolbarItems.Add(synchronizeButton);

			App.TodoItemsSource.Current.OnSynchroizationDone += updateList;

            Content = layout;

        }

		private void updateList(SynchronizationCompleteEventArgs<TodoItem> args)
        {
			Device.BeginInvokeOnMainThread (()=> 
				{
					var message = string.Format("C: {0}, D: {1}, N: {2}, U: {3} ", 
						args.Details.ConflictCount,
						args.Details.RemoveCount,
						args.Details.NewCount,
						args.Details.UpdateCount
					);

					Console.WriteLine("Nooovedades");
					args.Details.News.ToList()
						.ForEach (itm => Console.WriteLine(itm.Name));

					Console.WriteLine("Actualizaciones");
					args.Details.Updated.ToList()
						.ForEach (itm => Console.WriteLine(itm.Name));

					this.DisplayAlert("details",message,"cancel");
					ListView.ItemsSource = App.TodoItemsSource.Current.GetAllItems();
				}
			);
		}

        protected override void OnAppearing()
        {
			System.Diagnostics.Debug.WriteLine ("**** OnAppearing *****");

            base.OnAppearing();
            ListView.ItemsSource = App.TodoItemsSource.Current.GetAllItems();
        }
    }
}

