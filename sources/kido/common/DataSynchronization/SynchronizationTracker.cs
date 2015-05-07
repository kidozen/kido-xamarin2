using Couchbase.Lite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLitePCL;
using SQLitePCL.Ugly;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
    internal class SynchronizationTracker
    {
        String _dbPath;
        int _revsAtStart = -1;

        public int RevisionsAtStart
        {
            get { return _revsAtStart; }
            set { _revsAtStart = value; }
        }
        int _revsAtEnd = -1;

        public int RevisionsAtEnd
        {
            get { return _revsAtEnd; }
            set { _revsAtEnd = value; }
        }

        internal SynchronizationTracker(Database database)
        {
            _dbPath = String.Format("{0}/{1}.cblite", database.Manager.Directory, database.Name);
        }

        internal int GetAllDocumentsRevisionsCount()
        {
            var total = -1;
            using (var db = ugly.open(_dbPath))
            {
                total = db.query_scalar<int>("SELECT COUNT(*) FROM revs");
                db.close();
            }
            return total;
        }

        public void Start()
        {
            _revsAtStart = GetAllDocumentsRevisionsCount();
        }

        public void Stop()
        {
            _revsAtEnd = GetAllDocumentsRevisionsCount();
        }


    }
}
