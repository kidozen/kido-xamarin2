using Sync.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sync.Model
{
    public class TodoItemsSource
    {
        private ITodoItemRepository _repository = null;
        public ITodoItemRepository Current
        {
            get
            {
                return _repository;
            }
        }

        public TodoItemsSource(ITodoItemRepository repository)
        {
            _repository = repository;
        }

    }
}
