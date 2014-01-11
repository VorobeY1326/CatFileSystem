using System;
using System.Collections.Generic;
using System.Linq;

namespace CatFileSystem.FS
{
    /*
     * Блок с данными.
     * В памяти он хранится как:
     * байт для занятости, 4 байта для размера содержимого, само содержимое, 4 байта для номера следующего блока.
     * Размер блока всегда одинаков, номер следующего блока содержится в последних 4 байтах.
     */
    public class Block
    {
        public const int BlockDataSize = 1020;
        // Byte for fullness. 4 bytes for content size. Data block + 4 bytes for the next block offset
        public const int BlockSize = 1 + 4 + BlockDataSize + 4;

        public bool IsFull;
        public byte[] Content;
        public int? NextBlockId;

        // Десериализация
        public static Block FromBytes(byte[] blockBytes)
        {
            var dataSize = BitConverter.ToInt32(blockBytes, 1);

            return new Block
            {
                IsFull = BitConverter.ToBoolean(blockBytes, 0),
                Content = blockBytes.Skip(5).Take(dataSize).ToArray(),
                NextBlockId = ToBlockId(BitConverter.ToInt32(blockBytes, BlockSize - 4))
            };
        }

        private static int? ToBlockId(int id)
        {
            if (id == -1)
                return null;
            return id;
        }

        // Сериализация
        public byte[] ToBytes()
        {
            if (Content == null)
                Content = new byte[0];

            return BitConverter.GetBytes(IsFull)
                   .Concat(BitConverter.GetBytes(Content.Length))
                   .Concat(CompleteWithZeros(Content))
                   .Concat(BitConverter.GetBytes(FromBlockId(NextBlockId)))
                   .ToArray();
        }

        private IEnumerable<byte> CompleteWithZeros(byte[] content)
        {
            var additionalZerosCount = BlockDataSize - content.Length;
            return content.Concat(Enumerable.Repeat((byte)0, additionalZerosCount));
        }

        private static int FromBlockId(int? id)
        {
            if (id == null)
                return -1;
            return id.Value;
        }
    }
}