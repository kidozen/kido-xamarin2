using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLitePCL;
using SQLitePCL.Ugly;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using Couchbase.Lite;
using System.IO;
using System.Threading.Tasks;

#if __ANDROID__
namespace Kidozen.Android
#elif __IOS__
namespace Kidozen.iOS
#else
namespace Kidozen.DataSync
#endif
{
    internal class SynchronizationLog
    {
        internal const int SQLITE_OPEN_FILEPROTECTION_COMPLETEUNLESSOPEN = 0x00200000;
        internal const int SQLITE_OPEN_READONLY = 0x00000001;
        internal const int SQLITE_OPEN_READWRITE = 0x00000002;
        internal const int SQLITE_OPEN_CREATE = 0x00000004;
        internal const int SQLITE_OPEN_FULLMUTEX = 0x00010000;
        internal const int SQLITE_OPEN_NOMUTEX = 0x00008000;
        internal const int SQLITE_OPEN_PRIVATECACHE = 0x00040000;
        internal const int SQLITE_OPEN_SHAREDCACHE = 0x00020000;

        static string _kidoDatabaseInfoPath;
		Database _kidoDatabaseInfo;

        internal  SynchronizationLog(Database database)
        {
            _kidoDatabaseInfo = database;
            SynchronizationLog._kidoDatabaseInfoPath = Path.Combine(database.Manager.Directory, String.Format("{0}-kidoinfo.lite", database.Name));
        }

        internal  SynchronizationLog(string path)
        {
            SynchronizationLog._kidoDatabaseInfoPath = path;
        }

        void execute(sqlite3 db, string command)
        {
            Console.WriteLine(command);
            var error = string.Empty;
            db.exec(command, null, null, out error);
            if (!string.IsNullOrEmpty(error)) throw new Exception(error);
        }

        string getstring (sqlite3 db, string query) 
        {
            Console.WriteLine(query); 
            var error = string.Empty;
            var result = db.query_one_column<string>(query).FirstOrDefault();
            return result;
        }

        string _schema = "CREATE TABLE IF NOT EXISTS watermarks (tenant TEXT, app TEXT, datasync  TEXT, watermark TEXT )";
        string _newremote = "INSERT INTO watermarks (tenant,app,datasync, watermark) VALUES (\"{0}\",\"{1}\",\"{2}\",\"{3}\")";
        string _deleteremote = "DELETE FROM watermarks WHERE tenant = \"{0}\" AND app = \"{1}\" AND datasync = \"{2}\"";
        string _getwatermark = "SELECT watermark FROM watermarks WHERE tenant = \"{0}\" AND app = \"{1}\" AND datasync = \"{2}\"";

        internal Task<bool> WaterMarkHasChanged(string tenant, string app, string datasync, string watermark) {
            sqlite3 db;
            var rc = raw.sqlite3_open_v2(_kidoDatabaseInfoPath, out db, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, null);
            bool result = false;
            if (0 != rc) {
                raw.sqlite3_close(db);
                result = false;
            }
            var cmdDeleteRemote = string.Format(_deleteremote, tenant, app, datasync);
            var cmdInsertRemote = string.Format(_newremote,tenant,app, datasync, watermark);
            var queryWatermark = string.Format(_getwatermark, tenant,app, datasync);

            execute(db, _schema);
            var wm = getstring(db, queryWatermark);
            var changed = false;
            if (wm == null)
            {
                execute(db, cmdInsertRemote);
            }
            else
            {
                if (!wm.Equals(watermark))
	            {
                    execute(db, cmdDeleteRemote); 
                    execute(db, cmdInsertRemote); 
                    changed = true;
	            }
            }
            raw.sqlite3_close(db);
            return Task.FromResult<bool>(changed);
        }

    }
}
