using Couchbase.Lite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLitePCL;
using SQLitePCL.Ugly;
using System.Diagnostics;

#if __ANDROID__
namespace Kidozen.Android


#else
namespace Kidozen.iOS
#endif
{
	internal class SynchronizationTracker
	{
		String _dbPath;
		Database _db;

		internal SynchronizationTracker (Database database)
		{
			_db = database;
			_dbPath = String.Format ("{0}/{1}.cblite", database.Manager.Directory, database.Name);
		}

		internal IEnumerable<Revision> MapDocuments ()
		{
			Debugger.Break ();
			IEnumerable<Revision> results = null;
			try {
				using (var db = ugly.open (_dbPath)) {
					raw.sqlite3_create_collation (db, "JSON", null, CouchbaseSqliteJsonUnicodeCollationFunction.Compare);
					raw.sqlite3_create_collation (db, "JSON_ASCII", null, CouchbaseSqliteJsonAsciiCollationFunction.Compare);
					raw.sqlite3_create_collation (db, "JSON_RAW", null, CouchbaseSqliteJsonRawCollationFunction.Compare);
					raw.sqlite3_create_collation (db, "CURRENT", null, CouchbaseSqliteRevIdCollationFunction.Compare);
					raw.sqlite3_create_collation (db, "REVID", null, CouchbaseSqliteRevIdCollationFunction.Compare);

					results = db.query<Revision> ("SELECT sequence, doc_id, revid, parent, current, deleted, no_attachments FROM revs");
					db.close ();
				}
			} catch (Exception ex) {
				Debug.WriteLine (ex.Message);
			}
			return results;
		}

	}

	internal class Revision
	{
		public int sequence { get; set; }

		public int doc_id { get; set; }

		public string revid	{ get; set; }

		public string parent { get; set; }

		public bool current	{ get; set; }

		public bool deleted	{ get; set; }

		public bool no_attachments { get; set; }
	}


	//[Function(Name = "JSON", FuncType = FunctionType.Collation, Arguments = 2)]
	internal static class CouchbaseSqliteJsonUnicodeCollationFunction
	{
		/// <Docs>Implements the custom collection for JSON strings.</Docs>
		/// <summary>
		/// Couchbase custom JSON collation algorithm.
		/// </summary>
		/// <param name = "userData"></param>
		/// <param name = "param1"></param>
		/// <param name = "param2"></param>
		public static Int32 Compare (object userData, String param1, String param2)
		{
			return JsonCollator.Compare (JsonCollationMode.Unicode, param1, param2, Int32.MaxValue);
		}
	}

	//[SqliteFunction(Name = "JSON_ASCII", FuncType = FunctionType.Collation, Arguments = 2)]
	internal static class CouchbaseSqliteJsonAsciiCollationFunction
	{
		/// <Docs>Implements the custom collection for JSON strings.</Docs>
		/// <summary>
		/// Couchbase custom JSON collation algorithm.
		/// </summary>
		/// <remarks>
		/// This is woefully incomplete.
		/// For full details, see https://github.com/couchbase/couchbase-lite-ios/blob/580c5f65ebda159ce5d0ce1f75adc16955a2a6ff/Source/CBLCollateJSON.m.
		/// </remarks>
		/// <param name = "args"></param>
		public static Int32 Compare (object userData, String param1, String param2)
		{
			return JsonCollator.Compare (JsonCollationMode.Ascii, param1, param2, Int32.MaxValue);
		}
	}

	//[SqliteFunction(Name = "JSON_RAW", FuncType = FunctionType.Collation, Arguments = 2)]
	internal static class CouchbaseSqliteJsonRawCollationFunction
	{
		/// <Docs>Implements the custom collection for JSON strings.</Docs>
		/// <summary>
		/// Couchbase custom JSON collation algorithm.
		/// </summary>
		/// <remarks>
		/// This is woefully incomplete.
		/// For full details, see https://github.com/couchbase/couchbase-lite-ios/blob/580c5f65ebda159ce5d0ce1f75adc16955a2a6ff/Source/CBLCollateJSON.m.
		/// </remarks>
		/// <param name = "args"></param>
		public static Int32 Compare (object userData, String param1, String param2)
		{
			return JsonCollator.Compare (JsonCollationMode.Raw, param1, param2, Int32.MaxValue);
		}
	}

	//[SqliteFunction(Name = "REVID", FuncType = FunctionType.Collation, Arguments = 2)]
	internal static class CouchbaseSqliteRevIdCollationFunction
	{
		/// <Docs>Implements a custom collation for Revision ID strings.</Docs>
		/// <summary>
		/// Couchbase custom Revision ID collation algorithm.
		/// </summary>
		/// <remarks>
		/// For full details, see https://github.com/couchbase/couchbase-lite-ios/blob/master/Source/CBL_Revision.m
		/// </remarks>
		/// <param name = "args"></param>
		public static Int32 Compare (object userData, String param1, String param2)
		{
			return RevIdCollator.Compare (param1, param2);
		}
	}
}
