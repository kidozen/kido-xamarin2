using System;

namespace Kidozen.iOS
{
    class DataSyncException : Exception
    {
        private string p;

        public DataSyncException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }
    }
}
