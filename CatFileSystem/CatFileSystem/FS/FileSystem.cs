using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CatFileSystem.FS
{
    /*
     * Главная точка файловой системы, собственно файловая система.
     * Использует FilenamesHandler и BlocksHandler для работы с соответственно записями имён файлов и с блоками данных.
     */
    public class FileSystem
    {
        private readonly FilenamesHandler filenamesHandler;
        private readonly BlocksHandler blocksHandler;

        private const int filenamesTableSize = 20000;
        private const int blocksTableSize = 8000000;

        public FileSystem(string databaseFilename)
        {
            var database = new Database(databaseFilename, filenamesTableSize + blocksTableSize);
            filenamesHandler = new FilenamesHandler(database, filenamesTableSize);
            blocksHandler = new BlocksHandler(database, filenamesTableSize, blocksTableSize);

            if (!System.IO.File.Exists(databaseFilename))
                FormatC();
        }

        // Создает файл с файловой системой, если его не было, и удаляет всё, что там есть, форматируя таблицы имен файлов и блоков.
        public void FormatC()
        {
            filenamesHandler.EraseAll();
            blocksHandler.EraseAll();
        }

        // Находит имя файла и проходится по блокам с содержимым, собирая файл целиком, чтобы вернуть его
        public byte[] ReadFile(string filename)
        {
            var fileInfo = filenamesHandler.GetFileInfo(filename);

            if (!fileInfo.FileExists)
                return null;

            var result = new MemoryStream();

            foreach (var fileBlock in GetFileBlocks(filename))
                result.Write(fileBlock.Block.Content, 0, fileBlock.Block.Content.Length);

            return result.ToArray();
        }

        // Если файла не было, создает его
        // Потом стирает текущее содержимое файла и записывает набор новых блоков с данными,
        // предварительно получив набор пустых блоков, чтобы использовать их для записи.
        // Также обновляет запись имени файла новым первым блоком
        public void WriteFile(string filename, byte[] value)
        {
            if (!filenamesHandler.GetFileInfo(filename).FileExists)
                AddFile(filename);
            EraseFile(filename);

            if (value.Length == 0)
                return;

            var requiredBlocksCount = ( value.Length + Block.BlockDataSize - 1 ) / Block.BlockDataSize;
            var blockIds = blocksHandler.FoundFreeBlockNumbers().Take(requiredBlocksCount).ToArray();

            for (int i = 0; i < requiredBlocksCount; i++)
            {
                var blockSize = Math.Min(Block.BlockDataSize, value.Length - i*Block.BlockDataSize);
                var blockContent = new byte[blockSize];
                Array.Copy(value, i*Block.BlockDataSize, blockContent, 0, blockSize);
                int? nextBlockId = null;
                if (i < requiredBlocksCount - 1)
                    nextBlockId = blockIds[i + 1];

                var block = new Block
                {
                    IsFull = true,
                    Content = blockContent,
                    NextBlockId = nextBlockId
                };

                blocksHandler.SetBlock(block, blockIds[i]);
            }

            filenamesHandler.UpdateFileInfo(filename, blockIds[0]);
        }

        // Удаляет файл, если он был
        // Затем создает запись об имени файла
        public void AddFile(string filename)
        {
            RemoveFile(filename);
            filenamesHandler.Create(filename);
        }

        // Если файл существует, то освобождает его блоки и стирает запись об имени
        public void RemoveFile(string filename)
        {
            if (filenamesHandler.GetFileInfo(filename).FileExists)
            {
                EraseFile(filename);
                filenamesHandler.Remove(filename);
            }
        }

        private void EraseFile(string filename)
        {
            foreach (var fileBlock in GetFileBlocks(filename))
                blocksHandler.SetBlock(new Block{IsFull = false}, fileBlock.Id);

            filenamesHandler.UpdateFileInfo(filename, null);
        }

        // Возвращает все имена файлов в нашей файловой системе
        public IEnumerable<FilenameRecord> GetAllFilenames()
        {
            return filenamesHandler.GetExistingFilenames();
        }

        // Метод, реализующий логику прохождения по блокам файла
        private IEnumerable<BlockModel> GetFileBlocks(string filename)
        {
            var fileInfo = filenamesHandler.GetFileInfo(filename);

            if (!fileInfo.FileExists || fileInfo.FirstBlockId == null)
                yield break;

            var currentBlockNumber = fileInfo.FirstBlockId.Value;

            while (true)
            {
                var block = blocksHandler.GetBlock(currentBlockNumber);

                if (!block.IsFull)
                    throw new Exception("Corrupted filesystem, empty block found");

                yield return new BlockModel {Id = currentBlockNumber, Block = block};

                if (!block.NextBlockId.HasValue)
                    break;
                currentBlockNumber = block.NextBlockId.Value;
            }
        }
    }
}