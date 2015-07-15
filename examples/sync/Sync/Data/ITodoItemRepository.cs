using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Sync.Data
{
    public interface ITodoItemRepository
    {
        event SynchronizationDoneEventHandler OnSyncDone;

		Task<bool> AsyncAuthenticate();
        void Authenticate();
        IEnumerable<TodoItem> GetAllItems();
        IEnumerable<TodoItem> GetCompletedItems();
        void Upsert(TodoItem Item);
        void Delete(TodoItem Item);
        void Synchronize();
    }
}
