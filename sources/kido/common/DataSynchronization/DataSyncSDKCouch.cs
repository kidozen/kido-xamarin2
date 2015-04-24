using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Lite;

using A = KzApplication;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
    public partial class DataSync<T>
    {
        public delegate void SynchronizationCompleteEventHandler(object sender, SynchronizationEventArgs e);
        public event SynchronizationCompleteEventHandler OnSynchronizationComplete;

        public Database Database { get; set; }
        protected Query DefaultQuery { get; set; }
        protected Replication pullReplication = null;
        protected Replication pushReplication = null;

        public const string sufix = "master";

        private string _baseUrl = string.Empty;
        private string _tenant = string.Empty;

        public string BaseUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    _baseUrl = A.fetchConfigValue("datasync", _kidoapp.marketplace, _kidoapp.application, _kidoapp.key).Result;
                }

                return _baseUrl;
            }
        }

        public string Tenant
        {
            get
            {
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    _tenant = A.fetchConfigValue("tenant", _kidoapp.marketplace, _kidoapp.application, _kidoapp.key).Result;
                }
                return _tenant;
            }
        }

        private void SetupDatabase()
        {
            if (Database==null)
            {
                Database = Manager.SharedInstance.GetDatabase(this.Name);
            };
        }

        /// <summary>
        /// Gets replication url using the /publicapi endpoint. To replicate you must be logged in
        /// he replication endpoint is: datasync service url + 'rp' + tenant name + 'master' suffix
        /// </summary>
        /// <returns></returns>
        private string GetReplicationUrl()
        {
            var url = string.Format("{0}/{1}", BaseUrl, Name);
            System.Diagnostics.Debug.WriteLine(url);
            return url;
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
            var message = "======>Replicacion: {0}\nCompletedChangesCount: {1}\nChangesCount: {2}\n";
            System.Diagnostics.Debug.WriteLine(
                message,
                e.Source.IsPull ? "pull" : "push",
                e.Source.CompletedChangesCount,
                e.Source.ChangesCount
                );

            if (OnSynchronizationComplete!=null && e.Source.ChangesCount > 0)
            {
                if (e.Source.CompletedChangesCount == e.Source.ChangesCount)
                {
                    OnSynchronizationComplete.Invoke(this, new SynchronizationEventArgs());
                }
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

        public void Synchronize(bool Continuous = false, SynchronizationType synchronizationType = SynchronizationType.Pull)
        
        {
            var urlasstring = GetReplicationUrl();
            var url = new Uri(urlasstring);
            var headers = new Dictionary<string, string>();
            headers.Add("authorization", GetAuthHeaderValue());
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;
            switch (synchronizationType)
            {
                case SynchronizationType.Push:
                    setupAndStartPushReplication(url, headers, Continuous);
                    break;
                case SynchronizationType.Pull:
                    setupAndStartPullReplication(url, headers, Continuous);
                    break;
                case SynchronizationType.TwoWay:
                    setupAndStartPushReplication(url,headers, Continuous);
                    setupAndStartPullReplication(url,headers, Continuous);
                    break;
                default:
                    break;
            }

        }
        private void setupAndStartPushReplication(Uri url, IDictionary<string, string> headers, bool Continuous)
        {
            this.pushReplication = Database.CreatePushReplication(url);
            this.pushReplication.Continuous = Continuous;
            this.pushReplication.Headers = headers;
            this.pushReplication.Start();
            this.pushReplication.Changed += replication_Changed;
            //this.pullReplication.TransformationFunction = pushReplicationTransform;
        }
        private void setupAndStartPullReplication(Uri url, IDictionary<string, string> headers, bool Continuous)
        {
            this.pullReplication = Database.CreatePullReplication(url);
            this.pullReplication.Continuous = Continuous;
            this.pullReplication.Headers = headers;
            this.pullReplication.Start();
            this.pullReplication.Changed += replication_Changed;
            //this.pullReplication.TransformationFunction = pullReplicationTransform;
        }
        private IDictionary<string, object> pushReplicationTransform(IDictionary<string, object> propertyBag)
        {
            //TODO: Ensure document format To server is ok
            return propertyBag;
        }
        private IDictionary<string, object> pullReplicationTransform(IDictionary<string, object> propertyBag)
        {
            //TODO: Ensure document format from server is ok
            return propertyBag;
        }


    }
}
