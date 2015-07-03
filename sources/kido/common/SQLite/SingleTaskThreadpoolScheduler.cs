﻿using System;
using System.Collections.Generic; 
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Couchbase.Lite.Shared
{
    sealed internal class SingleTaskThreadpoolScheduler : TaskScheduler 
    {
        private const string Tag = "SingleTaskThreadpoolScheduler";
        private const int maxConcurrency = 1;

        private readonly BlockingCollection<Task> queue;
        private int runningTasks;

        public SingleTaskThreadpoolScheduler()
        {
            queue = new BlockingCollection<Task>(new System.Collections.Concurrent.ConcurrentQueue<Task>());
            runningTasks = 0;
        }

        /// <summary>Queues a task to the scheduler.</summary> 
        /// <param name="task">The task to be queued.</param> 
        protected override void QueueTask(Task task) 
        {
            // Long-running tasks can deadlock us easily.
            // We want to allow these to run without doing that.
            if (task.CreationOptions.HasFlag(TaskCreationOptions.LongRunning))
            {
                Debug.WriteLine(Tag, " --> Spawing a long running task as a thread.");
                var thread = new Thread((t)=>
                {
                    try {
                        var success = TryExecuteTask((Task)t);
                        if (!success)
                            throw new InvalidOperationException("A spawned task failed to run correctly.");
                    } catch (Exception ex) {
                        Debug.WriteLine(Tag, "Spawned task throw an unhandled exception.", ex);
                    }
                }) { IsBackground = true };
                thread.Start(task);
                return;
            }
            queue.Add (task); 
            Debug.WriteLine(Tag, " --> Queued a task: {0}/{1}", queue.Count, runningTasks);
            if (runningTasks < maxConcurrency)
            {
                Thread.MemoryBarrier();
                Interlocked.Increment(ref runningTasks);
                QueueThreadPoolWorkItem (); 
            }
        } 

        private void QueueThreadPoolWorkItem() 
        { 
            ThreadPool.QueueUserWorkItem(s => 
            { 
                try 
                { 
                    while (true) 
                    {
                        if (queue.Count == 0) 
                        {
                            Interlocked.Decrement(ref runningTasks);
                            Debug.WriteLine(Tag, " --> Exiting runloop: {0}/{1}", queue.Count, runningTasks);
                            break; 
                        } 
                        var task = queue.Take();
                        Debug.WriteLine(Tag, " --> Dequeued a task: {0}/{1}", queue.Count, runningTasks);
                        if (task.Status >= TaskStatus.Running)
                        {
                            Debug.WriteLine(Tag, "       skipping previously inlined task, which is still running.");
                        }
                        else
                        {
                            var success = TryExecuteTask(task);
                            if (task.Status == TaskStatus.Faulted)
                                Debug.WriteLine(Tag, "Scheduled task faulted", task.Exception);
                        }
                    } 
                }
                catch (Exception e)
                {
                    Debug.WriteLine(Tag, "Unhandled exception in runloop", e);
                    throw;
                }
            }, null);
        } 

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) 
        {
            Debug.WriteLine(Tag, "Executing task inline.");
            if (taskWasPreviouslyQueued) 
            {
                Debug.WriteLine(Tag, "Task was previously Queued, so expect it to error out later.");
                TryDequeue(task); 
            }

            var success = TryExecuteTask(task);
            if (!success && (task.Status != TaskStatus.Running && task.Status != TaskStatus.Canceled && task.Status != TaskStatus.RanToCompletion))
                Debug.WriteLine(Tag, "Scheduled task faulted", task.Exception);

            if (success && !task.IsCompleted)
            {
                //Mono (Android & iOS, at least) will throw an exception if this method returns true
                //before the task is complete
                success = task.Wait(TimeSpan.FromSeconds(10));
            }

            return success;
        } 

        protected override bool TryDequeue(Task task) 
        {
            // Our concurrent collection does not let
            // use efficiently re-order the queue,
            // so we won't try to.
            return false;
        } 

        public override int MaximumConcurrencyLevel { 
            get { 
                return maxConcurrency; 
            } 
        } 

        protected override IEnumerable<Task> GetScheduledTasks() 
        { 
            return queue.ToArray(); 
        }

        internal IEnumerable<Task> ScheduledTasks { get { return GetScheduledTasks(); } }
    } 
}