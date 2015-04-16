using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Lite;
using Kidozen.iOS.DataSynchronization;

namespace Kidozen.iOS
{
    public partial class DataSync<T>
    {
        public delegate void SynchronizationCompleteEventHandler(object sender, SynchronizationEventArgs e);
        public event SynchronizationCompleteEventHandler OnSynchronizationComplete;

        public Database Database { get; set; }
        protected Query DefaultQuery { get; set; }
        protected Replication pullReplication = null;
        protected Replication pushReplication = null;

        private void SetupDatabase()
        {
            if (Database==null)
            {
                Database = Manager.SharedInstance.GetDatabase(this.Name);
            };
        }

        /// <summary>
        /// TODO: Gets the url from the datasync configuration
        /// </summary>
        /// <returns></returns>
        private string GetReplicationUrl()
        {
            return "http://10.0.1.111:3000/" + Name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetAuthHeaderValue()
        {
            return _kidoapp.GetIdentity.rawToken;
        }

        void replication_Changed(object sender, ReplicationChangeEventArgs e)
        {
            if (OnSynchronizationComplete!=null)
            {
                OnSynchronizationComplete.Invoke(this, new SynchronizationEventArgs());
            }
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

        public void Synchronize(SynchronizationType synchronizationType= SynchronizationType.Pull) {
            var url = new Uri(GetReplicationUrl());
            var headers = new Dictionary<string, string>();
            headers.Add("PUSH-HEADER", GetAuthHeaderValue());

            switch (synchronizationType)
            {
                case SynchronizationType.Push:
                    this.pushReplication = Database.CreatePullReplication(url);
                    this.pushReplication.Start();
                    this.pushReplication.Changed += replication_Changed;
                    this.pushReplication.Headers = headers;
                    break;
                case SynchronizationType.Pull:
                    this.pullReplication = Database.CreatePushReplication(url);
                    this.pullReplication.Start();
                    this.pullReplication.Changed += replication_Changed;
                    this.pullReplication.Headers = headers;
                    break;
                case SynchronizationType.TwoWay:
                    break;
                default:
                    break;
            }


        }

    }
}
