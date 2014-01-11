using System;
using System.Text.RegularExpressions;
using CatFileSystem.FS;

namespace CatFileSystem.CLI
{
    /*
     * Парсер команды touch, создающей пустой файл.
     */
    public class TouchCommand : ICommand
    {
        public TouchCommand(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        private readonly FileSystem fileSystem;
        private readonly Regex touchCommandRegex = new Regex(@"^\S*touch\s+(\w+)\s*$");

        public bool CanExecute(string command)
        {
            return touchCommandRegex.IsMatch(command);
        }

        public void Execute(string command)
        {
            var match = touchCommandRegex.Match(command);

            if (match.Success)
            {
                var filename = match.Groups[1];
                fileSystem.AddFile(filename.Value);
            }
            else
                Console.WriteLine("Bad touch command: " + command);
        }
    }
}