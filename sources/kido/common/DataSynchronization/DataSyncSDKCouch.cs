﻿using System;
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
		private PullConflictResolutionType _pullConflictResolutionType;

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
			var url = string.Format ("{0}/{1}", BaseUrl, Name);
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
            //var message = "==****Fire Status: {0}\nfired: {1}\n";
            //Debug.WriteLine(message, e.Source.Status, _onSynchronizationStartFired);

			if (OnSynchronizationComplete != null && e.Source.Status == ReplicationStatus.Stopped) {
				_onSynchronizationStartFired = false;

				var details = CreateReplicationDetails ();
				if (_pullConflictResolutionType != PullConflictResolutionType.Default) {
					MaxRevisionConflictResolver ();
				}

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

		public void Synchronize ( SynchronizationType synchronizationType = SynchronizationType.Pull)
		{
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
			//this.pullReplication.TransformationFunction = pushReplicationTransform;
		}

		private void FireOnSynchronizationStart ()
		{
			if (OnSynchronizationStart != null && _onSynchronizationStartFired == false) {
				_documentsBeforeSync = _tracker.MapDocuments().ToList();

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
				return null; //TODO: What should I do?
			}
			catch (Exception ex) {
				return null; //TODO: What should I do?
			}
		}

		/// <summary>
		/// Resolves the last conflicts.
		/// </summary>
		/// <param name="winners">Winners.</param>
		public void ResolveLastConflicts(Func<IEnumerable<T>> winners) {
			throw new NotImplementedException ();
			MaxRevisionConflictResolver ();
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
