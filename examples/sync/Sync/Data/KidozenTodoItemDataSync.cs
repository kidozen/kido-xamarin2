using System;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Sync.Data;
using Kidozen;

#if __IOS__
using Kidozen.iOS;
#else
using Kidozen.Android;
#endif

namespace Sync.Data
{
    //To update UI
	public delegate void SynchronizationDoneEventHandler(SynchronizationCompleteEventArgs<TodoItem> args);

    public class KidozenTodoItemDataSync : ITodoItemRepository
    {
        KidoApplication kido = new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);
        DataSync<TodoItem> todoItems = null;
        private static KidozenTodoItemDataSync instance = null;

        public KidozenTodoItemDataSync()
        {

        }

        internal static ITodoItemRepository GetInstance()
        {
            if (instance==null)
            {
                instance = new KidozenTodoItemDataSync();
            }
            return instance;
        }

		public void SetupSyncCallbacks() {
			todoItems.OnSynchronizationComplete += onComplete;
			todoItems.OnSynchronizationStart += onStart;
			todoItems.OnSynchronizationProgress+= onProgress;
		}

		void onProgress (object sender, SynchronizationProgressEventArgs e)
		{
			//var message = string.Format ("onProgress , changesCount {0}, completed {1}, type {2}", e.ChangesCount, e.CompletedChangesCount, e.SynchronizationType);
			//Console.WriteLine(message);
		}

		void onStart (object sender, SynchronizationEventArgs e)
		{
			//var message = string.Format ("onStart , type {0}", e.SynchronizationType);
			//Console.WriteLine (message);
		}

		public void onComplete(object sender, SynchronizationCompleteEventArgs<TodoItem> e)
		{
			var message = string.Format ("onComplete , news {0}, updated {1}, deleted {2}, conflicts {3}", e.Details.NewCount, e.Details.UpdateCount, e.Details.RemoveCount, e.Details.ConflictCount);
			Console.WriteLine (message);
			//todoItems.ResolveLastConflicts (()=>
			//	e.Details.Conflicts.ToList().Where((itm)=> itm.Done)
			//);
			//todoItems.ResolveLastConflicts();
			if (OnSyncDone != null)
			{
				OnSyncDone.Invoke(e);
			}
		}

		public void NoAuthenticate() {
			todoItems = kido.DataSync<TodoItem> ("todolist");
			SetupSyncCallbacks ();
		}

		public void Authenticate() {
			if (!kido.IsAuthenticated) {
				var result = kido.Authenticate (Settings.User, Settings.Pass, Settings.Provider).Result;
				todoItems = kido.DataSync<TodoItem> ("todolist");
				SetupSyncCallbacks ();
			}
		}

		public Task<bool> AsyncAuthenticate()
        {
			var authTask = authTaskFactory().ContinueWith (
				t => {
					var success = !t.IsFaulted && kido.IsAuthenticated; 
					if (success) todoItems = kido.DataSync<TodoItem> ("todolist");
					SetupSyncCallbacks ();
					return success;
				}
			);

			return authTask;
        }



        public IEnumerable<TodoItem> GetAllItems()
        {
            return todoItems.Query();   
        }

        public IEnumerable<TodoItem> GetCompletedItems()
        {
            return todoItems.Query(i => i.Done == true);
        }

        public void Upsert(TodoItem Item)
        {
            todoItems.Save(Item);
        }

        public void Delete(TodoItem Item)
        {
            todoItems.Delete(Item);
        }


        public void Synchronize()
        {
			todoItems.Synchronize(SynchronizationType.Pull);
        }


        public event SynchronizationDoneEventHandler OnSyncDone;

		internal Task authTaskFactory() {
			#if __IOS__
			return kido.Authenticate();
			#else
			return kido.Authenticate(Xamarin.Forms.Forms.Context);
			#endif
		}
    }
}
