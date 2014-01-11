using System;
using System.Collections.Generic;

namespace CatFileSystem.FS
{
    /*
     * Обертка над FileSystem, обеспечивающая безопасный многопоточный доступ,
     * путем блокирования параллельной работы с файловой системой из разных потоков
     */
    public class SafeFileSystem
    {
        private readonly int threadId;
        private readonly Locker locker;
        private readonly FileSystem fileSystem;

        public SafeFileSystem(string databaseFilename, Locker locker, int threadId)
        {
            this.locker = locker;
            this.threadId = threadId;
            fileSystem = new FileSystem(databaseFilename);
        }

        public void FormatC()
        {
            ExecuteActionSafe(fileSystem.FormatC);
        }

        public byte[] ReadFile(string filename)
        {
            return ExecuteFuncSafe(() => fileSystem.ReadFile(filename));
        }

        public void WriteFile(string filename, byte[] value)
        {
            ExecuteActionSafe(() => fileSystem.WriteFile(filename, value));
        }

        public void AddFile(string filename)
        {
            ExecuteActionSafe(() => fileSystem.AddFile(filename));
        }

        public void RemoveFile(string filename)
        {
            ExecuteActionSafe(() => fileSystem.RemoveFile(filename));
        }

        // Единственный метод, которого нет в fileSystem и он не просто обертка.
        // Он реализован именно тут, потому что если цикл, идущий по названиям файлов, обернуть в критическую секцию,
        // то не будет никакой видимой конкуренции между потоками, а это не интересно.
        // Поэтому блокируем только операцию чтения файла, а цикл может и параллельно выполняться.
        public IEnumerable<File> GetAllFiles()
        {
            foreach (var filename in fileSystem.GetAllFilenames())
            {
                yield return new File
                {
                    Filename = filename.Filename,
                    Content = ExecuteFuncSafe(() => fileSystem.ReadFile(filename.Filename))
                };
            }
        }

        // Метод, выполняющий переданную ему функцию и возвращающий полученное значение
        // При этом он оборачивает этот вызов в безопасную однопоточную секцию
        private T ExecuteFuncSafe<T>(Func<T> func)
        {
            try
            {
                locker.Lock(threadId);
                return func();
            }
            finally
            {
                locker.Unlock(threadId);
            }
        }

        // Метод, выполняющий переданную ему функцию
        // При этом он оборачивает этот вызов в безопасную однопоточную секцию
        private void ExecuteActionSafe(Action action)
        {
            try
            {
                locker.Lock(threadId);
                action();
            }
            finally
            {
                locker.Unlock(threadId);
            }
        }
    }
}