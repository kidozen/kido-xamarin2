using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kidozen;
#if __IOS__
using Kidozen.iOS;
#else
using Kidozen.Android;
#endif
namespace Sync.Data
{
    class TodoItemModel
    {
        private static KidoApplication _kido;
        public static KidoApplication GetInstance()
        {
            if (_kido != null) return _kido;
            _kido = new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);
            return _kido;
        }

        private static DataSync<TodoItem> _myDataSync;
        public static DataSync<TodoItem> GetSyncItems()
        {
            if (_myDataSync != null) return _myDataSync;
            _myDataSync = GetInstance().DataSync<TodoItem>("items");
            return _myDataSync;
        }      

    }
}
