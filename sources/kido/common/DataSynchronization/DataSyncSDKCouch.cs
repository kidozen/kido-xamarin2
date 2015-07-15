﻿using System;
using System.Collections.Generic;
using System.Linq;

using A = KzApplication;
using System.Diagnostics;
using Couchbase.Lite;
using Couchbase.Lite.Auth;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if __ANDROID__
namespace Kidozen.Android
#elif __IOS__
namespace Kidozen.iOS
#else
namespace Kidozen.DataSync
#endif
{
	public partial class DataSync<T>
	{
		public delegate void SynchronizationCompleteEventHandler (object sender, SynchronizationCompleteEventArgs<T> e);

		public event SynchronizationCompleteEventHandler OnSynchronizationComplete;

		public delegate void SynchronizationStartEventHandler (object sender, SynchronizationEventArgs e);

		public event SynchronizationStartEventHandler OnSynchronizationStart;

		public delegate void SynchronizationProgressEventHandler (object sender, SynchronizationProgressEventArgs e);

		public event SynchronizationProgressEventHandler OnSynchronizationProgress;

		public Database Database { get; set; }

		protected Query DefaultQuery { get; set; }

		protected Replication pullReplication = null;
		protected Replication pushReplication = null;

		public const string sufix = "master";

		private string _baseUrl = string.Empty;
		private bool _onSynchronizationStartFired;

		private SynchronizationTracker _tracker;
		private IEnumerable<Revision> _documentsBeforeSync;
		private PullConflictResolutionType _pullConflictResolutionType;

		public string BaseUrl {
			get {
				if (string.IsNullOrEmpty (_baseUrl)) {
					_baseUrl = A.fetchConfigValue ("datasync", _kidoapp.marketplace, _kidoapp.application, _kidoapp.key).Result;
				}
				return _baseUrl;
			}
            set {
                _baseUrl = value;
            }
		}

		private void SetupDatabase ()
		{
			if (Database == null) {
                try
                {
                    Database = Manager.SharedInstance.GetDatabase(this.Name); 
                }
                catch (Exception e)
                {
                    throw;
                }
			}

		}
        //TODO: Quick and Dirty. Find a better way to code this
        private void ValidateWatermark()
        {
            var log = new SynchronizationLog(Database);
            var watermark = getWatermark();
            var x = Task.Factory.StartNew(
                () => log.WaterMarkHasChanged( _kidoapp.marketplace, _kidoapp.application, this.Name, watermark))
                .Result;
            if (x.Result)
            {
                this.Drop();
                Database = Manager.SharedInstance.GetDatabase(this.Name);
            }
        }

        private string getWatermark()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("authorization",this._kidoapp.CurrentUser.RawToken);
            var ep = BaseUrl + "/" + this.Name + "/_design/pk";
            var response = webClient.DownloadString(ep);
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(response, typeof(JObject));
            return jsonResponse["watermark"].Value<string>();
        }

		/// <summary>
		/// Gets replication url using the /publicapi endpoint. To replicate you must be logged in
		/// </summary>
		/// <returns></returns>
		private string GetReplicationUrl ()
		{
            var url = string.Format("{0}/{1}", BaseUrl, Name);
			System.Diagnostics.Debug.WriteLine (url);
			return url;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private string GetAuthHeaderValue ()
		{
			return _kidoapp.GetIdentity.rawToken;
		}

		void DebugSourceData (ReplicationChangeEventArgs e)
		{
			try {
				Debug.WriteLine ("e.Source.DocIds:" + e.Source.DocIds.ToList ().Count);

				e.Source.DocIds.ToList ().ForEach (id => {
					Console.WriteLine (id);
					Debug.WriteLine ("ParentId:");
					Debug.WriteLine (Database.GetExistingDocument (id).CurrentRevision.ParentId);

					Debug.WriteLine ("LeafRevisions:");
					Debug.WriteLine (Database.GetExistingDocument (id).LeafRevisions.FirstOrDefault ().ToString ());
				});
			} catch (Exception ex) {
				Debug.WriteLine (ex.Message);				
			}

		}
        
        #region Replication
        void replication_Changed (object sender, ReplicationChangeEventArgs e)
		{
            FireOnSynchronizationComplete(e);
            FireOnSynchronizationProgress (e);
		}

		private void FireOnSynchronizationProgress (ReplicationChangeEventArgs e)
		{
			if (OnSynchronizationProgress != null) {
				OnSynchronizationProgress.Invoke (
					this,
					new SynchronizationProgressEventArgs {
						CompletedChangesCount = e.Source.CompletedChangesCount,
						ChangesCount = e.Source.ChangesCount,
						SynchronizationType = e.Source.IsPull ? SynchronizationType.Pull : SynchronizationType.Push
					}
				);
			}
		}

		private void FireOnSynchronizationComplete (ReplicationChangeEventArgs e)
		{
			if (OnSynchronizationComplete != null && !e.Source.IsRunning ) {
				_onSynchronizationStartFired = false;

				var details = CreateReplicationDetails ();
				//if (_pullConflictResolutionType != PullConflictResolutionType.Default) {
				//	MaxRevisionConflictResolver ();
				//}

				OnSynchronizationComplete.Invoke (this,
					new SynchronizationCompleteEventArgs<T> {
						SynchronizationType = e.Source.IsPull ? SynchronizationType.Pull : SynchronizationType.Push,
						Details = details
					});
			}
		}
    
		//to simplify we are using ' all-docs query'
		protected void SetupDefaultQueryView ()
		{
			if (DefaultQuery == null) {
				DefaultQuery = Database.CreateAllDocumentsQuery ();
				DefaultQuery.AllDocsMode = AllDocsMode.AllDocs;   
			}
		}

		public void Synchronize ( SynchronizationType synchronizationType = SynchronizationType.Pull)
		{
            ValidateWatermark();
			if (synchronizationType!=SynchronizationType.Pull) {
				throw new NotSupportedException ("Current version only supports pull synchronizations.");
			}
			//For future use
			var Continuous = false;
			_pullConflictResolutionType =  PullConflictResolutionType.Default;
			//For future use

			_tracker = new SynchronizationTracker (this.Database);

			var url = new Uri (GetReplicationUrl ());
			var headers = new Dictionary<string, string> ();
			headers.Add ("authorization", GetAuthHeaderValue ());
			switch (synchronizationType) {
			case SynchronizationType.Push:
				setupAndStartPushReplication (url, headers, Continuous);
				break;
			case SynchronizationType.Pull:
				setupAndStartPullReplication (url, headers, Continuous);
				break;
			case SynchronizationType.TwoWay:
				setupAndStartPushReplication (url, headers, Continuous);
				setupAndStartPullReplication (url, headers, Continuous);
				break;
			default:
				break;
			}

		}

		private void setupAndStartPushReplication (Uri url, IDictionary<string, string> headers, bool Continuous)
		{
			this.pushReplication = Database.CreatePushReplication (url);
			this.pushReplication.Continuous = Continuous;
			this.pushReplication.Headers = headers;
			this.pushReplication.Start ();
			this.pushReplication.Changed += replication_Changed;
			FireOnSynchronizationStart ();
		}

		private void FireOnSynchronizationStart ()
		{
            _documentsBeforeSync = _tracker.MapDocuments().ToList();

			if (OnSynchronizationStart != null && _onSynchronizationStartFired == false) {
				_onSynchronizationStartFired = true;
				OnSynchronizationStart.Invoke (this, new SynchronizationEventArgs { SynchronizationType = SynchronizationType.TwoWay });
			}
		}

		private void setupAndStartPullReplication (Uri url, IDictionary<string, string> headers, bool Continuous)
		{
			this.pullReplication = Database.CreatePullReplication (url);
			this.pullReplication.Continuous = Continuous;
			this.pullReplication.Headers = headers;
            this.pullReplication.Filter = PullFilter;
            this.pullReplication.FilterParams = PullFilterParameters;
            this.pullReplication.Start ();
			this.pullReplication.Changed += replication_Changed;
            FireOnSynchronizationStart ();
		}

		QueryEnumerator GetConflicts ()
		{
			var onlyConflictsQuery = Database.CreateAllDocumentsQuery ();
			onlyConflictsQuery.AllDocsMode = AllDocsMode.OnlyConflicts;
			var onlyConflictsResults = onlyConflictsQuery.Run ();
			return onlyConflictsResults;
		}

		//Default Conflict resolver: based on this https://gist.github.com/jhs/1577159
		internal void MaxRevisionConflictResolver() {
			var documents = GetConflicts ();
			foreach (var doc in documents) {
				var coflictingRevisionsForDocument = doc.GetConflictingRevisions ();

				var maxid = coflictingRevisionsForDocument
					.Select (savedr => int.Parse (savedr.Id.Split ('-') [0]))
					.Max ();
				var highests = coflictingRevisionsForDocument.Where (cr => cr.Id.StartsWith (maxid.ToString ()));

				if (highests.Count () > 1) {
					var itemToDelete = coflictingRevisionsForDocument
						.Where (savedr => savedr.Id.StartsWith (maxid.ToString () + "-"))
						.OrderBy (sr => sr.Id)
						.FirstOrDefault ();
					highests = doc.GetConflictingRevisions ().Where (cr => cr.Id == itemToDelete.Id);
				}
				var toDelete = coflictingRevisionsForDocument.Except (highests);
				toDelete.ToList ().ForEach (td => td.Document.Delete ());
			}
		}

		//TODO: Only For Server2client????
		ReplicationDetails<T> CreateReplicationDetails () {
            var newFn = new Func<Revision, bool>(r => !r.docid.Contains("_design") && r.json!=null);
            var revisionComparer = new RevisionComparer();
            var docComparer = new DocumentIdComparer();
            var docAndrevisionComparer = new DocIdWithRevisionComparer();
            try {
                var docsBeforeSync = _documentsBeforeSync.Where(newFn);
                var documentsAfterSync = _tracker.MapDocuments().Where(newFn);

                var newRevisions = documentsAfterSync.Except(docsBeforeSync, docComparer).ToList();
                var updatedRevisions = documentsAfterSync.Except(docsBeforeSync, docAndrevisionComparer)
                    .Where(r=> !string.IsNullOrEmpty(r.parent));

                var deletedRevisions = _tracker.MapDocuments()
                    .Except(_documentsBeforeSync, revisionComparer)
                    .Where(d=> d.deleted);
                
                var docsConflicts = GetConflicts ().Select (r => new SyncDocument<T> ().DeSerialize<T> (r));
				var docsNews = QueryDocuments(newRevisions);
                var docsDeleted = QueryDocuments(deletedRevisions,includeDeleted:true);
				var docsUpdated = QueryDocuments(updatedRevisions);

				return new ReplicationDetails<T> {
					NewCount = docsNews.Count(),
					RemoveCount = docsDeleted.Count(),
					UpdateCount = docsUpdated.Count(),
					ConflictCount = docsConflicts.Count(),

					Conflicts = docsConflicts,
					News = docsNews,
					Deleted = docsDeleted,
					Updated = docsUpdated
				};
			}
			catch (SQLitePCL.Ugly.ugly.sqlite3_exception e) { return null; }
			catch (Exception ex) {return null;}
		}

		internal IEnumerable<T> QueryDocuments(IEnumerable<Revision> revisions, bool includeDeleted=false) {
			var allDocsQuery = Database.CreateAllDocumentsQuery ();
			allDocsQuery.AllDocsMode = AllDocsMode.AllDocs;
            allDocsQuery.IncludeDeleted = includeDeleted;
            allDocsQuery.Keys = revisions.Where(d => !d.docid.Contains("_design")).Select(r => r.docid);
			List<QueryRow> queryResults = new List<QueryRow>();
			try { queryResults = allDocsQuery.Run ().ToList();} 
			catch (ArgumentNullException ex) {//uncaught error in CouchBaseLite-net
				if (!ex.StackTrace.ToLower().Contains("couchbase.lite.queryenumerator.get_count")) {
					throw ex;
				}
			}
            if (includeDeleted)
                return QueryDeleted(queryResults).ToList();
            else
			    return DefaultQueryFilter (queryResults).ToList();
		}

        private IEnumerable<T> QueryDeleted(List<QueryRow> queryResults)
        {
            var documents =_tracker.MapDocuments();
            var docids = documents
                .Where(r => queryResults.Select(qr=>qr.DocumentRevisionId).Contains(r.revid))
                .Select(d => d.docid);
            var seqAndJson = documents
                .Where(r => docids.Contains(r.docid))
                .Select(d => new { d.docid, d.sequence, d.json, d.revid });
            var results = seqAndJson
                //.Where(d=> d.json != null)
                .GroupBy(i => i.docid)
                .Select(g => 
                    g.Aggregate((max, cur) =>
                            (max == null || cur.sequence > max.sequence) ? cur : max));

            return results.Select(r => 
                {
                    var d = (r.json == null) ? (T)Activator.CreateInstance(typeof(T)) : new SyncDocument<T>().DeSerialize<T>(r.json);
                    (d as DataSyncDocument)._id = r.docid;
                    (d as DataSyncDocument)._rev = r.revid;
                    return d;
                }
            );
        }

		/// <summary>
		/// Resolves the last conflicts.
		/// </summary>
		/// <param name="winners">Winners.</param>
		public void ResolveLastConflicts(Func<IEnumerable<T>> winners) {
			throw new NotImplementedException ();
			MaxRevisionConflictResolver ();
		}
		internal IEnumerable<T> DefaultQueryFilter(List<QueryRow> rows) {
			return 
				rows.Where(d=>d.Document.UserProperties!=null)
				.Where(d=>d.Document.UserProperties.ContainsKey("doc"))
				.Select (r => new SyncDocument<T> ().DeSerialize<T> (r));			
		}

        private const string DefaultPullFilter = "pullreplication/default";
        private string _pullFilter = string.Empty;
        public string PullFilter {
            get {
                return string.IsNullOrEmpty(_pullFilter) ? DefaultPullFilter : _pullFilter ;
            }
            set {
                _pullFilter = value;
            } 
        }

        private Dictionary<string, object> DefaultPullFilterParameters = new Dictionary<string, object>();
        private Dictionary<string, object> _pullFilterParameters = null;
        public  Dictionary<string, object> PullFilterParameters
        {
            get
            {
                DefaultPullFilterParameters = new Dictionary<string, object>();
                DefaultPullFilterParameters.Add("none", "0");

                return _pullFilterParameters == null ? DefaultPullFilterParameters : _pullFilterParameters;
            }
            set
            {
                _pullFilterParameters = value;
            }
        }


#endregion

        #region Query
        public IEnumerable<T> Query()
        {
            this.SetupDefaultQueryView();
            var results = DefaultQuery.Run();
            return DefaultQueryFilter(results.ToList());
        }

        public IEnumerable<T> Query(Func<T, bool> where)
        {
            return this.Query().ToList<T>().Where(where);
        }
        #endregion
    }

    internal class DocumentIdComparer : IEqualityComparer<Revision> {
        public bool Equals(Revision x, Revision y) {return x.doc_id == y.doc_id;}
        public int GetHashCode(Revision obj) {return obj.docid.GetHashCode();}
    }

	internal class RevisionComparer :IEqualityComparer<Revision>	{
		public bool Equals (Revision x, Revision y) { return x.revid == y.revid;}
		public int GetHashCode (Revision obj) {return obj.revid.GetHashCode();}
	}

    internal class DocIdWithRevisionComparer : IEqualityComparer<Revision> {
        public bool Equals(Revision x, Revision y) { return x.doc_id == y.doc_id && x.revid == y.revid;}
        public int GetHashCode(Revision obj) { return obj.docid.GetHashCode();}
    }

	public class ReplicationDetails<T>
	{
		public int NewCount { get; set; }
		public int RemoveCount { get; set; }
		public int UpdateCount { get; set; }
		public int ConflictCount { get; set; }

		public IEnumerable<T> News { get; set; }
		public IEnumerable<T> Deleted { get; set; }
		public IEnumerable<T> Updated { get; set; }
		public IEnumerable<T> Conflicts { get; set; }

	}
}
