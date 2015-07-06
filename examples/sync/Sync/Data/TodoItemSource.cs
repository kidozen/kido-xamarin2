using System.Collections.Generic;
using Kidozen;
#if __IOS__
using Kidozen.iOS;
#else
using Kidozen.Android;
#endif


// ReSharper disable once CheckNamespace
namespace Sync.Data
{
    public class TodoItemSource
    {
        KidoApplication _kido = new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);
        DataSync<TodoItem> todoItems;

        public TodoItemSource()
        {
             todoItems = _kido.DataSync<TodoItem>("items");
        }

        public static IEnumerable<TodoItem> GetItems()
        {
            return  todoItems.Query();   
        }

        public static IEnumerable<TodoItem> QueryItems()
        {
            return todoItems.Query(item => item.Id == 0);
        }

        public static string SaveItem(TodoItem item)
        {
            return todoItems.Save(item);
        }

        public static bool DeleteItem(TodoItem item)
        {
            return todoItems.Delete(item);
        }

        

    }


}
