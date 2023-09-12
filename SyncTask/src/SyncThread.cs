using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.CompilerServices;

namespace EasyEngine
{
    public static class SyncThread
    {
        public static readonly int threadId;
        private static ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();
        private static ConcurrentQueue<SyncTask> taskCompletionSources = new ConcurrentQueue<SyncTask>();
        public static event Action update;
        static SyncThread()
        {
            threadId = Thread.CurrentThread.ManagedThreadId;
            if (threadId != 1) throw new Exception($"SyncThread不在主线程 请注意查看 {threadId}");
        }
        public static void Init() { }
        public static void Update()
        {
            while (true)
            {
                if (!taskCompletionSources.TryDequeue(out var task))
                {
                    break;
                }
                try
                {
                    if (!task.SetResult())
                    {
                        taskCompletionSources.Enqueue(task);
                    }
                    else
                    {
                        SyncTask.Collect(task);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            while (true)
            {
                if (!queue.TryDequeue(out var a))
                {
                    break;
                }
                try
                {
                    a();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            try
            {
                update?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }


        /// <summary>
        /// 将异步线程等待到主线程
        /// </summary>
        public static async Task WaitMainThread()
        {
            if (Thread.CurrentThread.ManagedThreadId == threadId) return;
            SyncTask customTask = SyncTask.Get();
            taskCompletionSources.Enqueue(customTask);
            await customTask;
        }
        public static async Task<T> Wait<T>(Task<T> task)
        {
            var result = await task;
            await WaitMainThread();
            return result;
        }
        public static async Task Wait(Task task)
        {
            await task;
            await WaitMainThread();
        }
    }
}