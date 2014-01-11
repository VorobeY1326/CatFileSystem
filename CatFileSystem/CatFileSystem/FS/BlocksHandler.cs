using System;
using System.Collections.Generic;

namespace CatFileSystem.FS
{
    /*
     * Класс для работы с блоками, в которых содержится содержимое файлов.
     */
    public class BlocksHandler
    {
        private readonly Database database;
        private readonly long startOffset;
        private readonly int blocksCount;

        public BlocksHandler(Database database, long startOffset, int size)
        {
            this.database = database;
            this.startOffset = startOffset;
            blocksCount = size / Block.BlockSize;
        }

        public Block GetBlock(int number)
        {
            long offset = startOffset + number * Block.BlockSize;
            var blockBytes = database.Read(offset, Block.BlockSize);
            return Block.FromBytes(blockBytes);
        }

        public void SetBlock(Block block, int number)
        {
            long offset = startOffset + number * Block.BlockSize;
            var blockBytes = block.ToBytes();
            database.Write(offset, blockBytes);
        }

        // Возвращает все пустые номера блоков
        // Поскольку метод ленивый, он ищет ровно столько блоков, сколько понадобится тому, кто вызывает его
        public IEnumerable<int> FoundFreeBlockNumbers()
        {
            var i = 0;
            foreach (var blockBytes in database.ReadSequentially(startOffset, Block.BlockSize, blocksCount))
            {
                var isFull = BitConverter.ToBoolean(blockBytes, 0);
                if (!isFull)
                    yield return i;
                i++;
            }
        }

        public void EraseAll()
        {
            for (int i=0; i<blocksCount; i++)
                SetBlock(new Block {IsFull = false}, i);
        }
    }
}