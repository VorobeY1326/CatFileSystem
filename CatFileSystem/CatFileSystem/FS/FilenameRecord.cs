using System;
using System.Linq;
using System.Text;

namespace CatFileSystem.FS
{
    /*
     * Запись о имени файла.
     * В памяти она хранится как:
     * байт для занятости, имя файла, оканчивающееся нулём, если его длина меньше максимальной, и номер первого блока с данными.
     * Если первого блока с данными нет, то там лежит -1.
     */
    public class FilenameRecord
    {
        private const int maxFilenameLength = 60;
        // Byte for fullness, filename and 4 bytes for first block number
        public const int RecordSize = 1 + maxFilenameLength + 4;

        public bool IsFull;
        public string Filename;
        public int? FirstBlockNumber;

        // Сериализация
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(IsFull)
                    .Concat(Encoding.UTF8.GetBytes(ToZeroEncodedString(Filename)))
                    .Concat(BitConverter.GetBytes(EncodeNullableInt(FirstBlockNumber)))
                    .ToArray();
        }

        // Десериализация
        public static FilenameRecord FromBytes(byte[] recordBytes)
        {
            return new FilenameRecord
            {
                IsFull = BitConverter.ToBoolean(recordBytes, 0),
                Filename = FromZeroEncodedString(Encoding.UTF8.GetString(recordBytes, 1, maxFilenameLength)),
                FirstBlockNumber = DecodeNullableInt(BitConverter.ToInt32(recordBytes, RecordSize - 4))
            };
        }

        private static int EncodeNullableInt(int? firstBlockNumber)
        {
            if (firstBlockNumber.HasValue)
                return firstBlockNumber.Value;
            return -1;
        }

        private static int? DecodeNullableInt(int firstBlockNumber)
        {
            if (firstBlockNumber == -1)
                return null;
            return firstBlockNumber;
        }

        private static string FromZeroEncodedString(string s)
        {
            var zeroBytePos = s.IndexOf('\0');
            if (zeroBytePos != -1)
                s = s.Substring(0, zeroBytePos);
            return s;
        }

        private static string ToZeroEncodedString(string s)
        {
            if (s == null)
                return "";

            if (s.Length >= maxFilenameLength)
            {
                s = s.Substring(0, maxFilenameLength);
                return s;
            }

            var additionalZerosCount = maxFilenameLength - s.Length;
            return s + string.Join("", Enumerable.Repeat("\0", additionalZerosCount));
        }
    }
}