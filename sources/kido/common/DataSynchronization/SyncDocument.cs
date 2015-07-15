using System.Collections.Generic;
using Newtonsoft.Json;
using Couchbase.Lite;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

#if __ANDROID__
namespace Kidozen.Android
#elif __IOS__
namespace Kidozen.iOS
#else
namespace Kidozen.DataSync
#endif
{
    public class DocumentConstants
    {
        public static string ID_KEY = "_id";
        public static string REVISION_KEY = "_rev";
        public static string DOCUMENT_KEY = "doc";
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

		public SyncDocument() {
		}

		public SyncDocument(T document) {
			Document = document;
		}

        internal T DeSerialize<T>(byte[] doc)
        {
            var instance = JsonConvert.DeserializeObject<T>(
               Encoding.UTF8.GetString(doc)
            );
            return instance;
        }


        internal T DeSerialize<T>(QueryRow r)
        {
			var doqui = r.Document.Properties[DocumentConstants.DOCUMENT_KEY];

			var instance = JsonConvert.DeserializeObject<T>( 
				JsonConvert.SerializeObject(doqui)
			);

            var id = r.Document.Properties[DocumentConstants.ID_KEY].ToString();
            var revision = r.Document.Properties[DocumentConstants.REVISION_KEY].ToString();

			try {
				// !!!! ¿¿¿¿¿????? porque tengo que hacer esto para que funcione ?!!!
				var dsd = instance as DataSyncDocument;
				dsd._id = id;
				dsd._rev = revision;
			} catch (System.Exception ex) {
				Debug.WriteLine (ex.Message);
			}
			return instance;
        }

        internal IDictionary<string, object> ToCouchDictionary()
        {
            var dict = new Dictionary<string, object>();
            dict.Add(DocumentConstants.DOCUMENT_KEY,Document);
            return dict ;
        }
    }
}
