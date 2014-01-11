namespace CatFileSystem.FS
{
    /*
     * Информация о файле - есть ли он и его первый блок, если наш файл есть и файл непустой.
     */
    public class FileInfo
    {
        public bool FileExists { get; set; }
        public int? FirstBlockId { get; set; }
    }
}