using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            SyncThread.Init();

            for (int i = 0; i < 2; i++)
            {
                Test(i);
            }
            while (true)
            {
                SyncThread.Update();
                Thread.Sleep(1);
            }
        }

        private static async void Test(int index)
        {
            for (int i = 0; i < 2; i++)
            {
                var tmp = i;
                Debug.Log($"创建Task index:{index} tmp:{tmp}");
                await Task.Run(() =>
                {
                    Debug.Log($"执行Task index:{index} tmp:{tmp}");
                    Thread.Sleep(1000);
                });
                Debug.Log($"Task结束 异步线程 index:{index} tmp:{tmp}");
                await SyncThread.WaitMainThread();
                Debug.Log($"Task结束 回到主线程 index:{index} tmp:{tmp}");
            }
        }
    }
}
