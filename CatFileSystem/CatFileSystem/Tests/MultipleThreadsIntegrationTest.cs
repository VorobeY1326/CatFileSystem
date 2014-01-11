using System;
using System.Linq;
using CatFileSystem.FS;
using NUnit.Framework;
using File = System.IO.File;

namespace CatFileSystem.Tests
{
    /*
     * Тесты на многопоточную работу с файловой системой.
     */
    [TestFixture]
    public class MultipleThreadsIntegrationTest
    {
        private Random random;
        private int maxThreadsCount;
        private Locker locker;

        [SetUp]
        public void SetUp()
        {
            File.Delete("test.db");
            var fileSystem = new FileSystem("test.db");
            random = new Random();
            fileSystem.FormatC();
        }

        // Тестовая функция, запускаемая в каждом из потоков
        private void TryCreateRead(int threadId)
        {
            var fileSystem = new SafeFileSystem("test.db", locker, threadId);
            fileSystem.AddFile("qqq" + threadId);
            var value = Enumerable.Repeat((byte)random.Next(), 14).ToArray();
            fileSystem.WriteFile("qqq" + threadId, value);
            var actual = fileSystem.ReadFile("qqq" + threadId);
            
            Assert.AreEqual(value.Length, actual.Length);
            for (int i=0; i<value.LongLength; i++)
                Assert.AreEqual(value[i], actual[i]);
        }

        /*
         * Тест, в много параллельных потоков пишущий в файл и читающий из него, проверяя, что ничего не изменилось.
         */
        [Test]
        public void WriteReadTest()
        {
            maxThreadsCount = 256;
            locker = new Locker(maxThreadsCount);

            var threads = new CatThread[maxThreadsCount];
            for (int i=0; i<maxThreadsCount; i++)
                threads[i] = new CatThread(i, TryCreateRead);

            foreach (var thread in threads)
                thread.Run();

            foreach (var thread in threads)
               thread.Wait();
        }
    }
}