using System;
using System.Threading.Tasks;
using System.Threading;
using SQLitePCL;
using Kidozen.iOS;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SQLitePCL.Ugly;

#if !NET_3_5
using StringEx = System.String;
using Couchbase.Lite;
using Couchbase.Lite.Storage;
using Couchbase.Lite.Store;
#endif

namespace Couchbase.Lite.Shared
{
    internal sealed class SqlitePCLRawStorageEngine : ISQLiteStorageEngine, IDisposable
    {
        // NOTE: SqlitePCL.raw only defines a subset of the ones we want,
        // so we just redefine them here instead.
        private const int SQLITE_OPEN_FILEPROTECTION_COMPLETEUNLESSOPEN = 0x00200000;
        private const int SQLITE_OPEN_READONLY = 0x00000001;
        private const int SQLITE_OPEN_READWRITE = 0x00000002;
        private const int SQLITE_OPEN_CREATE = 0x00000004;
        private const int SQLITE_OPEN_FULLMUTEX = 0x00010000;
        private const int SQLITE_OPEN_NOMUTEX = 0x00008000;
        private const int SQLITE_OPEN_PRIVATECACHE = 0x00040000;
        private const int SQLITE_OPEN_SHAREDCACHE = 0x00020000;

        private const String Tag = "SqlitePCLRawStorageEngine";
        private sqlite3 _writeConnection;
        private sqlite3 _readConnection;

        private Boolean shouldCommit;

        private string Path { get; set; }
        private TaskFactory Factory { get; set; }
        private CancellationTokenSource _cts = new CancellationTokenSource();

        #region implemented abstract members of SQLiteStorageEngine

        public bool Open(String path)
        {
            if (IsOpen)
                return true;

            Path = path;
            Factory = new TaskFactory();

            var result = true;
            try
            {
                //Debug.WriteLine(Tag, "Sqlite Version: {0}".Fmt(raw.sqlite3_libversion()));
                
                shouldCommit = false;
                const int writer_flags = SQLITE_OPEN_FILEPROTECTION_COMPLETEUNLESSOPEN | SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE | SQLITE_OPEN_FULLMUTEX;
                OpenSqliteConnection(writer_flags, out _writeConnection);

                const int reader_flags = SQLITE_OPEN_FILEPROTECTION_COMPLETEUNLESSOPEN | SQLITE_OPEN_READONLY | SQLITE_OPEN_FULLMUTEX;
                OpenSqliteConnection(reader_flags, out _readConnection);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(Tag, "Error opening the Sqlite connection using connection String: {0}".Fmt(path), ex);
                result = false;
            }

            return result;
        }

        void OpenSqliteConnection(int flags, out sqlite3 db)
        {
            var status = raw.sqlite3_open_v2(Path, out db, flags, null);
            if (status != raw.SQLITE_OK)
            {
                Path = null;
                var errMessage = "Cannot open Sqlite Database at pth {0}" + (Path);
                throw new CouchbaseLiteException(errMessage, StatusCode.DbError);
            }
#if !__ANDROID__ && !NET_3_5 && VERBOSE
                var i = 0;
                var val = raw.sqlite3_compileoption_get(i);
                while (val != null)
                {
                    //Debug.WriteLine(Tag, "Sqlite Config: {0}".Fmt(val));
                    val = raw.sqlite3_compileoption_get(++i);
                }
#endif
            raw.sqlite3_create_collation(db, "JSON", null, CouchbaseSqliteJsonUnicodeCollationFunction.Compare);
            raw.sqlite3_create_collation(db, "JSON_ASCII", null, CouchbaseSqliteJsonAsciiCollationFunction.Compare);
            raw.sqlite3_create_collation(db, "JSON_RAW", null, CouchbaseSqliteJsonRawCollationFunction.Compare);
            raw.sqlite3_create_collation(db, "REVID", null, CouchbaseSqliteRevIdCollationFunction.Compare);
        }

        public Int32 GetVersion()
        {
            const string commandText = "PRAGMA user_version;";
            sqlite3_stmt statement;

            //NOTE.JHB Even though this is a read, iOS doesn't return the correct value on the read connection
            //but someone should try again when the version goes beyond 3.7.13
            statement = _writeConnection.prepare(commandText);

            var result = -1;
            try
            {
                var commandResult = raw.sqlite3_step(statement);
                if (commandResult != raw.SQLITE_ERROR)
                {
                    Debug.Assert(commandResult == raw.SQLITE_ROW);
                    result = raw.sqlite3_column_int(statement, 0);
                }
            }
            catch (Exception e)
            {
                //Debug.WriteLine(Tag, "Error getting user version", e);
            }
            finally
            {
                statement.Dispose();
            }

            return result;
        }

        public void SetVersion(Int32 version)
        {
            var errMessage = "Unable to set version to " + version;
            var commandText = "PRAGMA user_version = ?";

            Factory.StartNew(() =>
            {
                sqlite3_stmt statement = _writeConnection.prepare (commandText);
                if (raw.sqlite3_bind_int(statement, 1, version) == raw.SQLITE_ERROR)
                    throw new CouchbaseLiteException(errMessage, StatusCode.DbError);

                int result;
                try {
                    result = statement.step();
                    if (result != SQLiteResult.OK)
                        throw new CouchbaseLiteException(errMessage, StatusCode.DbError);
                } catch (Exception e) {
                    //Debug.WriteLine(Tag, "Error getting user version", e);
                } finally {
                    statement.Dispose();
                }
            });
        }

        public bool IsOpen
        {
            get
            {
                return _writeConnection != null;
            }
        }

        int transactionCount = 0;

        public int BeginTransaction()
        {
            if (!IsOpen)
            {
                Open(Path);
            }
            // NOTE.ZJG: Seems like we should really be using TO SAVEPOINT
            //           but this is how Android SqliteDatabase does it,
            //           so I'm matching that for now.
            var value = Interlocked.Increment(ref transactionCount);

            if (value == 1)
            {
                var t = Factory.StartNew(() =>
                {
					try {
						using (var statement = _writeConnection.prepare("BEGIN IMMEDIATE TRANSACTION"))
						{
							statement.step_done();
						}
					} catch (Exception e) {
							//Debug.WriteLine(Tag, "Error BeginTransaction", e);
					}
                });
                t.Wait();
            }

            return value;
        }

        public int EndTransaction()
        {
            if (_writeConnection == null)
                throw new InvalidOperationException("Database is not open.");

            var count = Interlocked.Decrement(ref transactionCount);
            if (count > 0)
                return count;

            /*if (_writeConnection == null)
            {
                if (shouldCommit)
                    throw new InvalidOperationException("Transaction missing.");
                return;
            }*/

            var t = Factory.StartNew(() =>
			{
				try {
					if (shouldCommit)
					{
						using (var stmt = _writeConnection.prepare("COMMIT"))
						{
							stmt.step_done();
						}
						shouldCommit = false;
					}
					else
					{
						using (var stmt = _writeConnection.prepare("ROLLBACK"))
						{
							stmt.step_done();
						}
					}
				} catch (Exception e) {
					//Debug.WriteLine(Tag, "Error EndTransaction", e);
				}
            });
            t.Wait();

            return 0;
        }

        public void SetTransactionSuccessful()
        {
            shouldCommit = true;
        }

        /// <summary>
        /// Execute any SQL that changes the database.
        /// </summary>
        /// <param name="sql">Sql.</param>
        /// <param name="paramArgs">Parameter arguments.</param>
        public void ExecSQL(String sql, params Object[] paramArgs)
        {  
            var t = Factory.StartNew(()=>
            {
                sqlite3_stmt command = null;
                    
                try
                {
                    command = BuildCommand(_writeConnection, sql, paramArgs);
                    var result = command.step();
                    if (result == SQLiteResult.ERROR)
                        throw new CouchbaseLiteException(raw.sqlite3_errmsg(_writeConnection), StatusCode.DbError);
                }
                catch (ugly.sqlite3_exception e)
                {
                    //Debug.WriteLine(Tag, "Error {0}, {1} executing sql '{2}'".Fmt(e.errcode, _writeConnection.extended_errcode(), sql), e);
                    throw;
                }
                finally
                {
                    if(command != null)
                    {
                        command.Dispose();
                    }
                }
            }, _cts.Token);

            try
            {
                //FIXME.JHB:  This wait should be optional (API change)
                t.Wait(30000, _cts.Token);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            catch (OperationCanceledException)
            {
                //Closing the storage engine will cause the factory to stop processing, but still
                //accept new jobs into the scheduler.  If execution has gotten here, it means that
                //ExecSQL was called after Close, and the job will be ignored.  Might consider
                //subclassing the factory to avoid this awkward behavior
                //Debug.WriteLine(Tag, "StorageEngine closed, canceling operation");
                return;
            }

            if (t.Status != TaskStatus.RanToCompletion) {
                //Debug.WriteLine(Tag, "ExecSQL timed out waiting for Task #{0}", t.Id);
                throw new CouchbaseLiteException("ExecSQL timed out", StatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Executes only read-only SQL.
        /// </summary>
        /// <returns>The query.</returns>
        /// <param name="sql">Sql.</param>
        /// <param name="paramArgs">Parameter arguments.</param>
     /*
        public Cursor IntransactionRawQuery(String sql, params Object[] paramArgs)
        {
            if (!IsOpen)
            {
                Open(Path);
            }

            if (transactionCount == 0) 
            {
                return RawQuery(sql, paramArgs);
            }

            var t = Factory.StartNew(() =>
            {
                Cursor cursor = null;
                sqlite3_stmt command = null;
                try 
                {
                    //Debug.WriteLine(Tag, "RawQuery sql: {0} ({1})", sql, String.Join(", ", paramArgs.ToStringArray()));
                    command = BuildCommand (_writeConnection, sql, paramArgs);
                    cursor = new Cursor(command);
                }
                catch (Exception e) 
                {
                    if (command != null) 
                    {
                        command.Dispose();
                    }
                    //Debug.WriteLine(Tag, "Error executing raw query '{0}'".Fmt(sql), e);
                    throw;
                }
                return cursor;
            });

            return t;
        }

    */
        /// <summary>
        /// Executes only read-only SQL.
        /// </summary>
        /// <returns>The query.</returns>
        /// <param name="sql">Sql.</param>
        /// <param name="paramArgs">Parameter arguments.</param>
        public Cursor RawQuery(String sql, params Object[] paramArgs)
        {
            if (!IsOpen)
            {
                Open(Path);
            }

            Cursor cursor = null;
            sqlite3_stmt command = null;

			var t = Factory.StartNew (() => 
			{
				try {
					//Debug.WriteLine (Tag, "RawQuery sql: {0} ({1})", sql, String.Join (", ", paramArgs.ToStringArray ()));
					command = BuildCommand (_readConnection, sql, paramArgs);
					cursor = new Cursor (command);
				} catch (Exception e) {
					if (command != null) {
						command.Dispose ();
					}
					var args = paramArgs == null 
                    ? String.Empty 
                    : String.Join (",", paramArgs.ToStringArray ());
					//Debug.WriteLine (Tag, "Error executing raw query '{0}' is values '{1}' {2}".Fmt (sql, args, _readConnection.errmsg ()), e);
					throw;
				}
				return cursor;
			});

			return t.Result;
        }

        public long Insert(String table, String nullColumnHack, ContentValues values)
        {
            return InsertWithOnConflict(table, null, values, ConflictResolutionStrategy.None);
        }

        public long InsertWithOnConflict(String table, String nullColumnHack, ContentValues initialValues, ConflictResolutionStrategy conflictResolutionStrategy)
        {
            if (!StringEx.IsNullOrWhiteSpace(nullColumnHack))
            {
                var e = new InvalidOperationException("{0} does not support the 'nullColumnHack'." + Tag);
                //Debug.WriteLine(Tag, "Unsupported use of nullColumnHack", e);
                throw e;
            }

            var t = Factory.StartNew(() =>
            {
                var lastInsertedId = -1L;
                var command = GetInsertCommand(table, initialValues, conflictResolutionStrategy);

                try
                {
                    int result;

                    result = command.step();
                    command.Dispose();
                    if (result == SQLiteResult.ERROR)
                        throw new CouchbaseLiteException(raw.sqlite3_errmsg(_writeConnection), StatusCode.DbError);

                    int changes = _writeConnection.changes();
                    if (changes > 0)
                    {
                        lastInsertedId = _writeConnection.last_insert_rowid();
                    }

                    if (lastInsertedId == -1L && conflictResolutionStrategy != ConflictResolutionStrategy.Ignore)
                    {
                        //Debug.WriteLine(Tag, "Error inserting " + initialValues + " using " + command);
                        throw new CouchbaseLiteException("Error inserting " + initialValues + " using " + command, StatusCode.DbError);
                    } else
                    {
                        //Debug.WriteLine(Tag, "Inserting row {0} into {1} with values {2}", lastInsertedId, table, initialValues);
                    }

                } 
                catch (Exception ex)
                {
                    //Debug.WriteLine(Tag, "Error inserting into table " + table, ex);
                    throw;
                }
                return lastInsertedId;
            });
            return t.Result;
        }

        public int Update(String table, ContentValues values, String whereClause, params String[] whereArgs)
        {
            Debug.Assert(!StringEx.IsNullOrWhiteSpace(table));
            Debug.Assert(values != null);

            var t = Factory.StartNew(() =>
            {
                var resultCount = 0;
                var command = GetUpdateCommand(table, values, whereClause, whereArgs);
                try
                {
                    var result = command.step();
                    if (result == SQLiteResult.ERROR)
                        throw new CouchbaseLiteException(raw.sqlite3_errmsg(_writeConnection),
                            StatusCode.DbError);
                }
                catch (ugly.sqlite3_exception ex)
                {
                    var msg = raw.sqlite3_extended_errcode(_writeConnection).ToString();
                    //Debug.WriteLine(Tag, "Error {0}: \"{1}\" while updating table {2}\r\n{3}", ex.errcode, msg, table, ex);
                }

                resultCount = _writeConnection.changes();
                if (resultCount < 0)
                {
                    //Debug.WriteLine(Tag, "Error updating " + values + " using " + command);
                    throw new CouchbaseLiteException("Failed to update any records.", StatusCode.DbError);
                }
                command.Dispose();
                return resultCount;
            }, CancellationToken.None);

            // NOTE.ZJG: Just a sketch here. Needs better error handling, etc.

            //doesn't look good
            var r = t.GetAwaiter().GetResult();
            if (t.Exception != null)
                throw t.Exception;
            return r;
        }

        public int Delete(String table, String whereClause, params String[] whereArgs)
        {
            Debug.Assert(!StringEx.IsNullOrWhiteSpace(table));

            var t = Factory.StartNew(() =>
            {
                var resultCount = -1;
                var command = GetDeleteCommand(table, whereClause, whereArgs);
                try
                {
                    var result = command.step();
                    if (result == SQLiteResult.ERROR)
                        throw new CouchbaseLiteException("Error deleting from table " + table, StatusCode.DbError);

                    resultCount = _writeConnection.changes();
                    if (resultCount < 0)
                    {
                        throw new CouchbaseLiteException("Failed to delete the records.", StatusCode.DbError);
                    }

                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(Tag, "Error {0} when deleting from table {1}".Fmt(_writeConnection.extended_errcode(), table), ex);
                    throw;
                }
                finally
                {
                    command.Dispose();
                }
                return resultCount;
            });

            // NOTE.ZJG: Just a sketch here. Needs better error handling, etc.
            var r = t.GetAwaiter().GetResult();
            if (t.Exception != null) 
            {
                //this is bad: should not arbitrarily crash the app
                throw t.Exception;
            }
                
            return r;
        }

        public void Close()
        {
            _cts.Cancel();
            Close(ref _readConnection);
            Close(ref _writeConnection);
            Path = null;
        }

        static void Close(ref sqlite3 db)
        {
            if (db == null)
            {
                return;
            }
            try
            {
                // Close any open statements, otherwise the
                // sqlite connection won't actually close.
                sqlite3_stmt next = null;
                while ((next = db.next_stmt(next))!= null)
                {
                    next.Dispose();
                } 
                db.close();
                //Debug.WriteLine(Tag, "db connection {0} closed", db);
            }
            catch (KeyNotFoundException ex)
            {
                // Appears to be a bug in sqlite3.find_stmt. Concurrency issue in static dictionary?
                // Assuming we're done.
                //Debug.WriteLine(Tag, "Abandoning database close.", ex);
            }
            catch (ugly.sqlite3_exception ex)
            {
                //Debug.WriteLine(Tag, "Retrying database close.", ex);
                // Assuming a basic retry fixes this.
                Thread.Sleep(5000);
                db.close();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                db.Dispose();
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(Tag, "Error while closing database.", ex);
            }
            finally
            {                
                db = null;
            }
        }

        #endregion

        #region Non-public Members
        private sqlite3_stmt BuildCommand(sqlite3 db, string sql, object[] paramArgs)
        {
            sqlite3_stmt command = null;
            try
            {
                if (!IsOpen)
                {
                    if(Open(Path) == false)
                    {
                        throw new CouchbaseLiteException("Failed to Open " + Path, StatusCode.DbError);
                    }
                }

                int err = raw.sqlite3_prepare_v2(db, sql, out command);

				if (err != raw.SQLITE_OK)
				{
					//Debug.WriteLine(Tag, "sqlite3_prepare_v2: " + err);
				}

                if (paramArgs.Length > 0 && command != null && err != raw.SQLITE_ERROR)
                {
                    command.bind(paramArgs);
                }
            }
            catch (Exception e)
            {
                //Debug.WriteLine(Tag, "Error when build a sql " + sql + " with params " + paramArgs, e);
                throw;
            }
            return command;
        }

        /// <summary>
        /// Avoids the additional database trip that using SqliteCommandBuilder requires.
        /// </summary>
        /// <returns>The update command.</returns>
        /// <param name="table">Table.</param>
        /// <param name="values">Values.</param>
        /// <param name="whereClause">Where clause.</param>
        /// <param name="whereArgs">Where arguments.</param>
        sqlite3_stmt GetUpdateCommand(string table, ContentValues values, string whereClause, string[] whereArgs)
        {
            if (!IsOpen)
            {
                Open(Path);
            }
            var builder = new StringBuilder("UPDATE ");

            builder.Append(table);
            builder.Append(" SET ");

            // Append our content column names and create our SQL parameters.
            var valueSet = values.ValueSet();

            var paramList = new List<object>();

            var index = 0;
            foreach (var column in valueSet)
            {
                if (index++ > 0)
                {
                    builder.Append(",");
                }
                builder.AppendFormat("{0} = ?", column.Key);
                paramList.Add(column.Value);
            }

            if (!StringEx.IsNullOrWhiteSpace(whereClause))
            {
                builder.Append(" WHERE ");
                builder.Append(whereClause);
            }

            if (whereArgs != null)
            {
                paramList.AddRange(whereArgs);
            }

            var sql = builder.ToString();
            var command = _writeConnection.prepare(sql);
            command.bind(paramList.ToArray<object>());

            return command;
        }

        /// <summary>
        /// Avoids the additional database trip that using SqliteCommandBuilder requires.
        /// </summary>
        /// <returns>The insert command.</returns>
        /// <param name="table">Table.</param>
        /// <param name="values">Values.</param>
        /// <param name="conflictResolutionStrategy">Conflict resolution strategy.</param>
        sqlite3_stmt GetInsertCommand(String table, ContentValues values, ConflictResolutionStrategy conflictResolutionStrategy)
        {
            if (!IsOpen)
            {
                Open(Path);
            }
            var builder = new StringBuilder("INSERT");

            if (conflictResolutionStrategy != ConflictResolutionStrategy.None)
            {
                builder.Append(" OR ");
                builder.Append(conflictResolutionStrategy);
            }

            builder.Append(" INTO ");
            builder.Append(table);
            builder.Append(" (");

            // Append our content column names and create our SQL parameters.
            var valueSet = values.ValueSet();
            var valueBuilder = new StringBuilder();
            var index = 0;

            var args = new object[valueSet.Count];

            foreach (var column in valueSet)
            {
                if (index > 0)
                {
                    builder.Append(",");
                    valueBuilder.Append(",");
                }

                builder.AppendFormat("{0}", column.Key);
                valueBuilder.Append("?");

                args[index] = column.Value;

                index++;
            }

            builder.Append(") VALUES (");
            builder.Append(valueBuilder);
            builder.Append(")");

            var sql = builder.ToString();
            sqlite3_stmt command = null;
            if (args != null)
            {
                //Debug.WriteLine(Tag, "Preparing statement: '{0}' with values: {1}", sql, String.Join(", ", args.Select(o => o == null ? "null" : o.ToString()).ToArray()));
            }
            else
            {
                //Debug.WriteLine(Tag, "Preparing statement: '{0}'", sql);
            }
            command = _writeConnection.prepare(sql);
            command.bind(args);               

            return command;
        }

        /// <summary>
        /// Avoids the additional database trip that using SqliteCommandBuilder requires.
        /// </summary>
        /// <returns>The delete command.</returns>
        /// <param name="table">Table.</param>
        /// <param name="whereClause">Where clause.</param>
        /// <param name="whereArgs">Where arguments.</param>
        sqlite3_stmt GetDeleteCommand(string table, string whereClause, string[] whereArgs)
        {
            if (!IsOpen)
            {
                Open(Path);
            }
            var builder = new StringBuilder("DELETE FROM ");
            builder.Append(table);
            if (!StringEx.IsNullOrWhiteSpace(whereClause))
            {
                builder.Append(" WHERE ");
                builder.Append(whereClause);
            }

            sqlite3_stmt command;
            command = _writeConnection.prepare(builder.ToString());
            command.bind(whereArgs);

            return command;
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
