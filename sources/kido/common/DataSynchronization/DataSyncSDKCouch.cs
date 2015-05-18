using System;
using System.Collections.Generic;
using System.Linq;

using A = KzApplication;
using System.Diagnostics;
using Couchbase.Lite;

#if __ANDROID__
namespace Kidozen.Android


#else
namespace Kidozen.iOS
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
		private string _tenant = string.Empty;
		private bool _onSynchronizationStartFired;

		private SynchronizationTracker _tracker;
		private IEnumerable<Revision> _documentsBeforeSync;

		public string BaseUrl {
			get {
				if (string.IsNullOrEmpty (_baseUrl)) {
					_baseUrl = A.fetchConfigValue ("datasync", _kidoapp.marketplace, _kidoapp.application, _kidoapp.key).Result;
				}

				return _baseUrl;
			}
		}

		public string Tenant {
			get {
				if (string.IsNullOrEmpty (_baseUrl)) {
					_tenant = A.fetchConfigValue ("tenant", _kidoapp.marketplace, _kidoapp.application, _kidoapp.key).Result;
				}
				return _tenant;
			}
		}

		private void SetupDatabase ()
		{
			if (Database == null) {
				Database = Manager.SharedInstance.GetDatabase (this.Name);
			}
			;
		}

		/// <summary>
		/// Gets replication url using the /publicapi endpoint. To replicate you must be logged in
		/// he replication endpoint is: datasync service url + 'rp' + tenant name + 'master' suffix
		/// </summary>
		/// <returns></returns>
		private string GetReplicationUrl ()
		{
			var url = string.Format ("{0}/{1}", "http://Christians-MacBook-Pro.local:5984", Name);
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

				}
				);

			} catch (Exception ex) {
				Debug.WriteLine (ex.Message);				
			}

		}

		void replication_Changed (object sender, ReplicationChangeEventArgs e)
		{
			/*
			var message = "======>Replicacion: {0}\nCompletedChangesCount: {1}\nChangesCount: {2}\n Status: {3}\n";
			System.Diagnostics.Debug.WriteLine (
				message,
				e.Source.IsPull ? "pull" : "push",
				e.Source.CompletedChangesCount,
				e.Source.ChangesCount,
				e.Source.Status
			);
			*/
			//DebugSourceData (e);

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
            var message = "==****Fire Status: {0}\nfired: {1}\n";
            Debug.WriteLine(message, e.Source.Status, _onSynchronizationStartFired);

			if (OnSynchronizationComplete != null && e.Source.Status == ReplicationStatus.Stopped) {
				_onSynchronizationStartFired = false;

				var details = CreateReplicationDetails ();

				OnSynchronizationComplete.Invoke (this,
					new SynchronizationCompleteEventArgs<T> {
						SynchronizationType = e.Source.IsPull ? SynchronizationType.Pull : SynchronizationType.Push,
						Details = details
					});
			}
		}
    

		//Para simplificar la Beta, uso solo ' all-docs query'
		//hay que agregar en futuras versiones soportar customs views
		protected void SetupDefaultQueryView ()
		{
			if (DefaultQuery == null) {
				DefaultQuery = Database.CreateAllDocumentsQuery ();
				DefaultQuery.AllDocsMode = AllDocsMode.AllDocs;   
			}
		}

		//Return all
		public IEnumerable<T> Query ()
		{
			this.SetupDefaultQueryView ();
			var results = DefaultQuery.Run ();
			return results.Select (r => new SyncDocument<T> ().DeSerialize<T> (r));
		}

		public IEnumerable<T> Query (Func<T,bool> where)
		{
			return this.Query ().ToList<T> ().Where (where);
		}

		public void Synchronize (SynchronizationType synchronizationType = SynchronizationType.Pull)
		{
			var Continuous = false;
            
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
			//this.pullReplication.TransformationFunction = pushReplicationTransform;
		}

		private void FireOnSynchronizationStart ()
		{
			if (OnSynchronizationStart != null && _onSynchronizationStartFired == false) {
				//TODO: Aca? o cuando la transacciones estan en 0 ????
				Debug.WriteLine("FireOnSynchronizationStart: ");

				_documentsBeforeSync = _tracker.MapDocuments().ToList();
				//TODO: Aca? o cuando la transacciones estan en 0 ????

				_onSynchronizationStartFired = true;
				OnSynchronizationStart.Invoke (this, new SynchronizationEventArgs { SynchronizationType = SynchronizationType.TwoWay });
			}
		}

		private void setupAndStartPullReplication (Uri url, IDictionary<string, string> headers, bool Continuous)
		{
			this.pullReplication = Database.CreatePullReplication (url);
			this.pullReplication.Continuous = Continuous;
			this.pullReplication.Headers = headers;
			this.pullReplication.Start ();
			this.pullReplication.Changed += replication_Changed;
			FireOnSynchronizationStart ();
			//this.pullReplication.TransformationFunction = pullReplicationTransform;
		}

		private IDictionary<string, object> pushReplicationTransform (IDictionary<string, object> propertyBag)
		{
			//TODO: Ensure document format To server is ok
			return propertyBag;
		}

		private IDictionary<string, object> pullReplicationTransform (IDictionary<string, object> propertyBag)
		{
			//TODO: Ensure document format from server is ok
			return propertyBag;
		}

		QueryEnumerator GetConflicts ()
		{
			var onlyConflictsQuery = Database.CreateAllDocumentsQuery ();
			onlyConflictsQuery.AllDocsMode = AllDocsMode.OnlyConflicts;
			var onlyConflictsResults = onlyConflictsQuery.Run ();
			return onlyConflictsResults;
		}

		//Default Conflict resolver:
		//Create a new revision with the properties of the latest current revision (285) and deletes all other revisions.
		internal void DefaultConflictResolver() {
			var documents = GetConflicts ();
			foreach (var doc in documents) 
			{
				doc.GetConflictingRevisions ().ToList().ForEach(r =>
					{
						Debug.WriteLine( "User Properties " );
						r.UserProperties.ToList() .ForEach ( i =>
							Debug.WriteLine(String.Format("Key: {0} ;Value={1}", i.Key, i.Value))
						);
						Debug.WriteLine( "Doc Id: " + r.Document.Id  );
						Debug.WriteLine( r.Document.CurrentRevisionId ?? "CurrentRevisionId is NULL");
						Debug.WriteLine( "Deleted: " + r.Document.Deleted );
						Debug.WriteLine( "Deleted: " + r.Document.GetRevision(r.Document.Id)  ?? "GetRevision() is NULL" );
						Debug.WriteLine( "Doc Properties " );
						r.Properties.ToList() .ForEach ( i =>
							Debug.WriteLine(String.Format("Key: {0} ;Value={1}", i.Key, i.Value))
						);
					}
				);
				/*	
				var conflictingRevisions = doc.GetConflictingRevisions ().ToList();
				if (conflictingRevisions.Any())
				{
					conflictingRevisions.ForEach ( revision => {
						var sequence = doc.SequenceNumber;
						var userProperties =  revision.UserProperties;
						Database.RunInTransaction (()=>
							{
								var currentRevision = doc.Document.CurrentRevision;
								var newRevision = revision.CreateRevision();
								if (revision==currentRevision) {
									newRevision.SetUserProperties(userProperties);
								}
								else {
									newRevision.IsDeletion=true;
								}
								return true;
							});
					});
				}
				*/
			}
		}

		//TODO: Only For Server2client????
		//TODO: Should we resolve conflicts automatically? Does the Server has priority over local??
		private ReplicationDetails<T> CreateReplicationDetails ()
		{
			var onlyConflictsResults = GetConflicts ();

			var conflictsCount = onlyConflictsResults.ToList ().Count;
		
			var documents = new Func<Revision, bool>(r=>r.current);
			var updates = new Func<Revision, bool>(r=>!r.current);

			try
			{
				var documentsAfterSync = _tracker.MapDocuments().ToList();
				var countBeforeSync = _documentsBeforeSync.Where(documents).ToList().Count();

				var countAfterSync =  documentsAfterSync.Where(documents).Count();
				var totalDeleted = countBeforeSync - countAfterSync;
				var totalNews = countAfterSync - countBeforeSync;

				var newsAsRevision = documentsAfterSync
					.Where(documents).Except
					(
						_documentsBeforeSync.Where(documents).ToList()
						, new RevisionComparer()
					);

				var updatedDocuments = documentsAfterSync
					.Where(updates).Except
					(
						_documentsBeforeSync.Where(updates).ToList()
						,new RevisionComparer()
					)
					.GroupBy(rev => rev.doc_id)
					.Select(grp => grp.First());

				var deletedAsRevision = _documentsBeforeSync
					.Where(documents).Except
					(
						documentsAfterSync.Where(documents).ToList()
						, new RevisionComparer()
					);
				
				//updatedDocuments.ToList().ForEach(d=>Debug.WriteLine("diff: " + d.doc_id));

				return new ReplicationDetails<T>
				{
					NewCount = totalNews < 0 ? 0 : totalNews,
					RemoveCount = totalDeleted < 0 ? 0 : totalDeleted,
					UpdateCount = updatedDocuments.Count(),
					ConflictCount = conflictsCount,

					Conflicts = onlyConflictsResults.Select (r => new SyncDocument<T> ().DeSerialize<T> (r)),
					News = QueryDocuments(newsAsRevision),
					Deleted = QueryDocuments(deletedAsRevision),
					Updated = QueryDocuments(updatedDocuments)
				};
			}
			catch (SQLitePCL.Ugly.ugly.sqlite3_exception e)
			{
				return null;
			}
			catch (Exception ex) {
				return null;
			}
		}

		/// <summary>
		/// Resolves the conflicts.
		/// </summary>
		/// <param name="discardLocal">If set to <c>true</c> discard local changes.</param>
		public void ResolveLastConflicts(Boolean discardLocal = true) {
			DefaultConflictResolver ();
		}

		public void ResolveLastConflicts(Func<IEnumerable<T>> winners) {
			DefaultConflictResolver ();
		}

		internal IEnumerable<T> QueryDocuments(IEnumerable<Revision> revisions) {
			var allDocsQuery = Database.CreateAllDocumentsQuery ();
			allDocsQuery.AllDocsMode = AllDocsMode.AllDocs;
			allDocsQuery.Keys = revisions.Select (r => r.docid);

			List<QueryRow> queryResults = new List<QueryRow>();
			try {
				queryResults = allDocsQuery.Run ().ToList();
			} 
			//uncaught error in CouchBaseLite-net
			catch (ArgumentNullException ex) {
				if (!ex.StackTrace.ToLower().Contains("couchbase.lite.queryenumerator.get_count")) {
					throw ex;
				}
			}
			return queryResults.Select (r => new SyncDocument<T> ().DeSerialize<T> (r));
		}
	}

	internal class RevisionComparer :IEqualityComparer<Revision>	{
		#region IEqualityComparer implementation
		public bool Equals (Revision x, Revision y)
		{
			return x.doc_id == y.doc_id;
		}
		public int GetHashCode (Revision obj)
		{
			return String.Format("{0}|{1}",obj.doc_id, obj.revid).GetHashCode();
		}
		#endregion
		
	}
}
