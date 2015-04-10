using System;

namespace Kidozen.iOS
{
    public partial class DataSync<T>
    {
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

        //TODO: Check how to delete. Seems that 'deletelocaldocument' does not work (?)
        public bool Delete(T instance)
        {
            var document = instance as DataSyncDocument;
            var localdocument = Database.GetDocument(document._id);
            if (localdocument.UserProperties == null) return false;
            
            localdocument.Delete();
            return true;
        }

        public void Synchronize()
        {
            throw new NotImplementedException();
        }

        public void Drop()
        {
            Database.Delete();
        }
    }
}
