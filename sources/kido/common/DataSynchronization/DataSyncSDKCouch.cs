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
        public delegate void SynchronizationCompleteEventHandler(object sender, SynchronizationCompleteEventArgs e);
        public event SynchronizationCompleteEventHandler OnSynchronizationComplete;

        public delegate void SynchronizationStartEventHandler(object sender, SynchronizationEventArgs e);
        public event SynchronizationStartEventHandler OnSynchronizationStart;

        public delegate void SynchronizationProgressEventHandler(object sender, SynchronizationProgressEventArgs e);
        public event SynchronizationProgressEventHandler OnSynchronizationProgress;

        public Database Database { get; set; }
        protected Query DefaultQuery { get; set; }
        protected Replication pullReplication = null;
        protected Replication pushReplication = null;

        public const string sufix = "master";

        private string _baseUrl = string.Empty;
        private string _tenant = string.Empty;
        private bool _onSynchronizationStartFired;

        private SynchronizationTracker _tracker;

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
            var url = string.Format("{0}/{1}", "http://10.0.1.111:5984", Name);
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
            FireOnSynchronizationProgress(e);

            FireOnSynchronizationComplete(e);
        }

        private void FireOnSynchronizationProgress(ReplicationChangeEventArgs e)
        {

            if (OnSynchronizationProgress != null)
            {
                OnSynchronizationProgress.Invoke(
                    this,
                    new SynchronizationProgressEventArgs
                    {
                        CompletedChangesCount = e.Source.CompletedChangesCount,
                        ChangesCount = e.Source.ChangesCount,
                        SynchronizationType = e.Source.IsPull ? SynchronizationType.Pull : SynchronizationType.Push
                    }
                );
            }
        }

        private void FireOnSynchronizationComplete(ReplicationChangeEventArgs e)
        {
            if (OnSynchronizationComplete != null && e.Source.ChangesCount > 0)
            {
                if (e.Source.CompletedChangesCount == e.Source.ChangesCount)
                {
                    _tracker.Stop();
                    _onSynchronizationStartFired = false;
                    var details = CreateReplicationDetails();
                    OnSynchronizationComplete.Invoke(this,
                        new SynchronizationCompleteEventArgs
                        {
                            SynchronizationType = e.Source.IsPull ? SynchronizationType.Pull : SynchronizationType.Push,
                            Details = details
                        });
                }
            }
        }
        //TODO: Only For Server2client????
        //TODO: Should we resolve conflicts automatically? Does the Server has priority over local??
        private ReplicationDetails CreateReplicationDetails()
        {
            var totalConflicts = 0;
            if (_tracker.RevisionsAtStart==_tracker.RevisionsAtEnd)
            {
                var conflict = Database.CreateAllDocumentsQuery();
                conflict.AllDocsMode = Couchbase.Lite.AllDocsMode.OnlyConflicts;
                totalConflicts = conflict.Run().ToList().Count;
            }
            return new ReplicationDetails
            {
                News = _tracker.RevisionsAtEnd - _tracker.RevisionsAtStart,
                Deleted = _tracker.RevisionsAtStart - _tracker.RevisionsAtEnd,
                Updated = totalConflicts
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

        public void Synchronize(SynchronizationType synchronizationType = SynchronizationType.Pull)
        {
            var Continuous = false;
            
            _tracker = new SynchronizationTracker(this.Database);

            var url = new Uri(GetReplicationUrl());
            var headers = new Dictionary<string, string>();
            headers.Add("authorization", GetAuthHeaderValue());
            switch (synchronizationType)
            {
                case SynchronizationType.Push:
                    setupAndStartPushReplication(url, headers, Continuous);
                    break;
                case SynchronizationType.Pull:
                    setupAndStartPullReplication(url, headers, Continuous);
                    break;
                case SynchronizationType.TwoWay:
                    setupAndStartPushReplication(url, headers, Continuous);
                    setupAndStartPullReplication(url, headers, Continuous);
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
            FireOnSynchronizationStart();
            //this.pullReplication.TransformationFunction = pushReplicationTransform;
        }

        private void FireOnSynchronizationStart()
        {
            if (OnSynchronizationStart != null && _onSynchronizationStartFired==false)
            {
                //TODO: Aca? o cuando la transacciones estan en 0 ????
                _tracker.Start();
                //TODO: Aca? o cuando la transacciones estan en 0 ????

                _onSynchronizationStartFired = true;
                OnSynchronizationStart.Invoke(this, new SynchronizationEventArgs { SynchronizationType = SynchronizationType.TwoWay });
            }
        }

        private void setupAndStartPullReplication(Uri url, IDictionary<string, string> headers, bool Continuous)
        {
            this.pullReplication = Database.CreatePullReplication(url);
            this.pullReplication.Continuous = Continuous;
            this.pullReplication.Headers = headers;
            this.pullReplication.Start();
            this.pullReplication.Changed += replication_Changed;
            FireOnSynchronizationStart();
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
