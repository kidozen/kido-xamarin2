using Newtonsoft.Json;
using System.Collections.Generic;
using System;

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
        KidoApplication _kidoapp;
        string _name;
        public string Name { 
            get {return  _name;} 
            set {_name = value.EnsureOnlyASCII();} 
        }

        public DataSync(string name)
        {
            this.Name = name;
            this.SetupDatabase();
        }

        public DataSync(string name, KidoApplication app) :this(name)
        {
            // TODO: Complete member initialization
            
            this._kidoapp = app;
        }

        /// <summary>
        /// Creates a new document in local database
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Couch revision id</returns>
        public string Create(T instance)
        {
            var document = new SyncDocument<T> { Document = instance }.ToCouchDictionary();
            var dbDocument = Database.CreateDocument();
            var rev = dbDocument.PutProperties(document);
         
            return rev.Id;
        }
        /// <summary>
        /// Updates an existing document using underlying putProperties method
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Couch revision id</returns>
        public string Update(T instance)
        {
            var document = new SyncDocument<T> { Document = instance }.ToCouchDictionary();

            var id = (instance as DataSyncDocument)._id;
            var retrieved = Database.GetDocument(id);
            
            var properties = new Dictionary<string,object>(retrieved.Properties);
            properties[DocumentConstants.DOCUMENT_KEY] = document[DocumentConstants.DOCUMENT_KEY];
            var rev = retrieved.PutProperties(properties);

            return rev.Id;
        }
        /// <summary>
        /// Upserts a document
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public string Save(T instance)
        {
            var id = (instance as DataSyncDocument)._id;
            if (id!=null)
            {
                return this.Update(instance);   
            }
            else
            {
                return this.Create(instance);
            }
        }

        //TODO: Check how to delete. Seems that 'deletelocaldocument' does not work (?)
        public bool Delete(T instance)
        {
            var document = instance as DataSyncDocument;
            var localdocument = Database.GetDocument(document._id);
            if (localdocument.UserProperties == null) return false;
            
            localdocument.Delete();
            return true;
        }

        public void Drop()
        {
            Database.Delete();
        }
    }
}
