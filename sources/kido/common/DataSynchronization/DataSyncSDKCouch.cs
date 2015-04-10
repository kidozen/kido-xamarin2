using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Lite;

namespace Kidozen.iOS
{
    public partial class DataSync<T>
    {
        public Database Database { get; set; }
        protected Query DefaultQuery { get; set; }

        private void SetupDatabase()
        {
            if (Database==null)
            {
                Database = Manager.SharedInstance.GetDatabase(this.Name);
            };
        }

        //Para simplificar la Beta, uso solo ' all-docs query' 
        //hay que agregar en futuras versiones soportar customs views
        protected void SetupDefaultQueryView (){
            if (DefaultQuery == null)
            {
                DefaultQuery = Database.CreateAllDocumentsQuery();
                DefaultQuery.AllDocsMode = AllDocsMode.AllDocs;   
            }
        }

        //Return all
        public IEnumerable<T> Query() {
            this.SetupDefaultQueryView();
            var results = DefaultQuery.Run();
            return results.Select(r => new SyncDocument<T>().DeSerialize<T>(r));
        }
        
        public IEnumerable<T> Query(Func<T,bool> where)
        {
            return this.Query().ToList<T>().Where(where);
        }

    }
}
