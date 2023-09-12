using System;
using System.Runtime.CompilerServices;
using DesperateDevs.Caching;

namespace EasyEngine
{
    /*
        SyncTask:专门为SyncThread.WaitMainThread创建的安全Task
        什么是SyncTask呢?
        已知一个Task可以进行await 同时可以对外提供一个SetResult方法
        正常情况下 在执行await task之后
        就会触发UnsafeOnCompleted函数或者OnCompleted函数
        目前OnCompleted函数还没遇见过
        触发UnsafeOnCompleted函数是什么意思呢,就是将await task后边的代码行封装成为一个方法传递进来
        然后在主线程调用SetResult,顺势执行通过UnsafeOnCompleted函数传递过来的continuation
        上述,就可以将一个本来在异步线程的代码行转换到主线程处理
        
        但是!!!
        由于异步线程的先后执行顺序不固定,就会出现先调用SetResult,后调用UnsafeOnCompleted的情况
        常规做法是调用UnsafeOnCompleted的时候检测一下是否该task是否已经完成,如果完成则直接执行方法
        这样就会导致UnsafeOnCompleted函数传递过来的continuation还是在异步线程执行
        为了解决这种问题 创建了SyncTask来进行解决
    */
    public class SyncTask : ICriticalNotifyCompletion
    {
        private static ObjectPool<SyncTask> pool = new ObjectPool<SyncTask>(() =>
        {
            return new SyncTask();
        }, (task) =>
        {
            task._IsCompleted = false;
            task.continuation = null;
        });

        public static SyncTask Get()
        {
            lock (pool)
            {
                return pool.Get();
            }
        }
        public static void Collect(SyncTask safeTask)
        {
            lock (pool)
            {
                pool.Push(safeTask);
            }
        }


        private bool _IsCompleted = false;
        public bool IsCompleted => _IsCompleted;
        private Action continuation;
        public SyncTask GetAwaiter()
        {
            return this; //相当于省略了awaitable，MyTask自己就是一个awaiter
        }
        public void GetResult()
        {

        }

        public bool SetResult()
        {
            if (_IsCompleted) throw new System.Exception("已经完成的Task不能执行SetResult");
            if (continuation == null) return false;
            _IsCompleted = true;
            continuation?.Invoke();
            return true;
        }

        public void OnCompleted(Action continuation)
        {
            throw new Exception("OnCompleted 暂时未遇到触发的现象");
            // this.continuation = continuation;
        }
        public void UnsafeOnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
    }



    public class SyncTask<T> : ICriticalNotifyCompletion
    {
        private static ObjectPool<SyncTask<T>> pool = new ObjectPool<SyncTask<T>>(() =>
        {
            return new SyncTask<T>();
        }, (task) =>
        {
            task._IsCompleted = false;
            task.continuation = null;
        });
        public static SyncTask<T> Get()
        {
            lock (pool)
            {
                return pool.Get();
            }
        }
        public static void Push(SyncTask<T> safeTask)
        {
            lock (pool)
            {
                pool.Push(safeTask);
            }
        }


        private bool _IsCompleted = false;
        public bool IsCompleted => _IsCompleted;
        private Action continuation;
        private T result;
        public SyncTask<T> GetAwaiter()
        {
            return this; //相当于省略了awaitable，MyTask自己就是一个awaiter
        }
        public void GetResult()
        {

        }

        public bool SetResult(T result)
        {
            if (_IsCompleted) throw new System.Exception("已经完成的Task不能执行SetResult");
            if (continuation == null) return false;
            this.result = result;
            _IsCompleted = true;
            continuation?.Invoke();
            return true;
        }

        public void OnCompleted(Action continuation)
        {
            throw new Exception("OnCompleted 暂时未遇到触发的现象");
            // this.continuation = continuation;
        }
        public void UnsafeOnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
    }
}