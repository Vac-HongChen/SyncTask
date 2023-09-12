using System.Threading;
using System;

namespace EasyEngine
{
    public static partial class Debug
    {
        private static ConsoleColor defaultForegroundColor;

        public static void Log(object o)
        {
            
            System.Console.ForegroundColor = ConsoleColor.White;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            System.Console.WriteLine($"{DateTime.Now} 线程:{threadId} {o}");
        }
        public static void LogError(object o)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            System.Console.WriteLine($"{DateTime.Now} 线程:{threadId} {o}");
            System.Console.ForegroundColor = defaultForegroundColor;
        }
    }
}