using System.Collections.Generic;
using Couchbase.Lite;
using Newtonsoft.Json;

namespace Kidozen.iOS
{
    public class DocumentConstants
    {
        public static string ID_KEY = "_id";
        public static string REVISION_KEY = "_rev";
        public static string DOCUMENT_KEY = "doc";
        public static string PRIMARYKEY_KEY = "key";
    }

    public class SyncDocument<T>
    {
        /// <summary>
        /// The Document or Entity defined by the user
        /// </summary>
        public T Document { get; set; }
        /// <summary>
        /// The KidoZen defined primary key
        /// </summary>
        protected string key { get; set; }
        /// <summary>
        /// The KidoZen defined document
        /// </summary>
        protected string doc { 
            get {
                return JsonConvert.SerializeObject(this.Document);        
            }  
        }
        /// <summary>
        /// The couchbase _id propertu
        /// </summary>
        protected string _id { get; set; }
        /// <summary>
        /// The couchbase _ref property
        /// </summary>
        protected string _ref { get; set; }

        internal T DeSerialize<T>(QueryRow r)
        {
            var doc = r.Document.Properties[DocumentConstants.DOCUMENT_KEY];
            var instance = JsonConvert.DeserializeObject<T>(doc.ToString());
            var id = r.Document.Properties[DocumentConstants.ID_KEY].ToString();
            var revision = r.Document.Properties[DocumentConstants.REVISION_KEY].ToString();

            var dsd = instance as DataSyncDocument;
            dsd._id = id;
            dsd._rev = revision;

            return instance;
        }


        internal IDictionary<string, object> ToCouchDictionary()
        {
            var dict = new Dictionary<string, object>();
            //dict.Add(DocumentConstants.ID_KEY, _id.ValueOrEmpty());
            //dict.Add(DocumentConstants.REF_KEY, _ref.ValueOrEmpty());
            dict.Add(DocumentConstants.PRIMARYKEY_KEY, key.ValueOrEmpty());
            dict.Add(DocumentConstants.DOCUMENT_KEY, doc.ValueOrEmpty());
            return dict ;
        }
    }
}
