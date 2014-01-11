using System;
using System.Text.RegularExpressions;
using CatFileSystem.FS;
using CatFileSystem.Tests;

namespace CatFileSystem.CLI
{
    /*
     * Парсер команды test, запускающей тест на многопоточность.
     * Тест запускает несколько потоков, каждый из которых проходит по FS и выводит размер каждого файла.
     */
    public class TestCommand : ICommand
    {
        private readonly string dbFileName;

        public TestCommand(string dbFileName)
        {
            this.dbFileName = dbFileName;
        }

        private readonly Regex testCommandRegex = new Regex(@"^\s*test\s*$");

        public bool CanExecute(string command)
        {
            return testCommandRegex.IsMatch(command);
        }

        private const int threadsCount = 10;
        private Locker locker;

        private void ReadAllFiles(int threadId)
        {
            var fileSystem = new SafeFileSystem(dbFileName, locker, threadId);
            foreach (var file in fileSystem.GetAllFiles())
            {
                Console.WriteLine("Thread {0} reports that {1} size is {2}", threadId, file.Filename, file.Content.Length);
            }
        }

        public void Execute(string command)
        {
            locker = new Locker(threadsCount);

            var threads = new CatThread[threadsCount];
            for (int i=0; i<threadsCount; i++)
                threads[i] = new CatThread(i, ReadAllFiles);

            for (int i = 0; i < threadsCount; i++)
                threads[i].Run();

            for (int i = 0; i < threadsCount; i++)
                threads[i].Wait();
        }
    }
}