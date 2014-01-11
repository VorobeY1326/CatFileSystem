using System.Collections.Generic;
using System.IO;

namespace CatFileSystem.FS
{
    /*
     * Наиболее низкоуровневый класс для работы с файловой системой.
     * Он умеет только писать/читать блоки информации в файл с нашей файловой системой.
     */
    public class Database
    {
        private readonly string filename;

        public Database(string filename, long fileSize)
        {
            this.filename = filename;

            if (!System.IO.File.Exists(filename))
            {
                using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                    stream.SetLength(fileSize);
            }
        }

        public byte[] Read(long offset, int length)
        {
            var result = new byte[length];

            using (var stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(result, 0, length);
            }

            return result;
        }

        public void Write(long offset, byte[] what)
        {
            using (var stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Write(what, 0, what.Length);
            }
        }

        // Метод для чтения нескольких блоков одинакого размера подряд.
        // Полезен, например, для поиска свободного блока.
        public IEnumerable<byte[]> ReadSequentially(long initialOffset, int blockSize, int count)
        {
            using (var stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Seek(initialOffset, SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    var result = new byte[blockSize];
                    stream.Read(result, 0, blockSize);
                    yield return result;
                }
            }
        }
    }
}