using System;
using System.Collections.Generic;
using System.Linq;

namespace CatFileSystem.FS
{
    /*
     * Класс для работы с записями об именах файлов.
     */
    public class FilenamesHandler
    {
        private readonly Database database;
        private readonly int recordsCount;

        public FilenamesHandler(Database database, int filenamesTableSize)
        {
            this.database = database;
            recordsCount = filenamesTableSize / FilenameRecord.RecordSize;
        }

        // Проходится по записям и ищет запись с указанным названием файла
        public FileInfo GetFileInfo(string filename)
        {
            foreach (var filenameRecord in GetFilenameRecords())
            {
                if (filenameRecord.IsFull && filenameRecord.Filename == filename)
                    return new FileInfo {FileExists = true, FirstBlockId = filenameRecord.FirstBlockNumber};
            }
            return new FileInfo {FileExists = false};
        }

        // Проходится по записям и обновляет запись с указанным названием файла
        public void UpdateFileInfo(string filename, int? firstBlockNumber)
        {
            var i = 0;
            foreach (var filenameRecord in GetFilenameRecords())
            {
                if (filenameRecord.IsFull && filenameRecord.Filename == filename)
                {
                    filenameRecord.FirstBlockNumber = firstBlockNumber;
                    WriteRecord(filenameRecord, i);
                    return;
                }
                i++;
            }
            throw new Exception(string.Format("Filename {0} not found", filename));
        }

        // Проходится по записям и удаляет запись с указанным названием файла
        public void Remove(string filename)
        {
            var i = 0;
            foreach (var filenameRecord in GetFilenameRecords())
            {
                if (filenameRecord.IsFull && filenameRecord.Filename == filename)
                {
                    WriteRecord(new FilenameRecord {IsFull = false}, i);
                    return;
                }
                i++;
            }
        }

        // Проходится по записям и ищет свободную запись , чтобы записать туда указанное название файла
        public void Create(String filename)
        {
            var record = new FilenameRecord
            {
                IsFull = true,
                Filename = filename,
                FirstBlockNumber = null
            };

            var i = 0;
            foreach (var filenameRecord in GetFilenameRecords())
            {
                if (!filenameRecord.IsFull)
                {
                    WriteRecord(record, i);
                    return;
                }
                i++;
            }

            throw new Exception("Filenames count exceeded");
        }

        // Проходится по записям и возвращает все непустые
        public IEnumerable<FilenameRecord> GetExistingFilenames()
        {
            return GetFilenameRecords().Where(r => r.IsFull);
        }

        private void WriteRecord(FilenameRecord record, int recordNumber)
        {
            long offset = recordNumber * FilenameRecord.RecordSize;
            var recordBytes = record.ToBytes();
            database.Write(offset, recordBytes);
        }

        private IEnumerable<FilenameRecord> GetFilenameRecords()
        {
            foreach (var recordBytes in database.ReadSequentially(0, FilenameRecord.RecordSize, recordsCount))
            {
                yield return FilenameRecord.FromBytes(recordBytes);
            }
        }

        public void EraseAll()
        {
            for (int i=0; i<recordsCount; i++)
                WriteRecord(new FilenameRecord{IsFull = false}, i);
        }
    }
}