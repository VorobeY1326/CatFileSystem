using System;
using System.Threading.Tasks;

namespace CatFileSystem.Tests
{
    /*
     * Простая обертка над стандартными потоками, в которую можно передавать номер потока, необходимый для алгоритма блокировки.
     */
    public class CatThread
    {
        private readonly int threadId;
        private readonly Action<int> func;
        private Task task;

        public CatThread(int threadId, Action<int> func)
        {
            this.threadId = threadId;
            this.func = func;
        }

        public void Run()
        {
            task = new Task(() => func(threadId));
            task.Start();
        }

        public void Wait()
        {
            task.Wait();
        }
    }
}