namespace CatFileSystem.FS
{
    /*
     * Все, что представляет собой файл: имя и содержимое.
     */
    public class File
    {
        public string Filename { get; set; }
        public byte[] Content { get; set; }
    }
}