using System;
using System.Text.RegularExpressions;
using CatFileSystem.FS;

namespace CatFileSystem.CLI
{
    /*
     * Парсер команды rm, удаляющей файл.
     */
    public class RmCommand : ICommand
    {
        public RmCommand(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        private readonly FileSystem fileSystem;
        private readonly Regex rmCommandRegex = new Regex(@"^\s*rm\s+(\S+)\s*$");

        public bool CanExecute(string command)
        {
            return rmCommandRegex.IsMatch(command);
        }

        public void Execute(string command)
        {
            var match = rmCommandRegex.Match(command);

            if (match.Success)
            {
                var filename = match.Groups[1];
                fileSystem.RemoveFile(filename.Value);
            }
            else
                Console.WriteLine("Bad RM command: " + command);
        }
    }
}